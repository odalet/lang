const std = @import("std");
const Token = @import("token.zig").Token;
const Loc = @import("loc.zig").Loc;
const Diagnostics = @import("diagnostics.zig").Diagnostics;

pub const Lexer = struct {
    const Self = @This();

    const Result = union(enum) {
        token: Token,
        eof: void,
    };

    allocator: std.mem.Allocator,
    source: []const u8,
    offset: usize,
    line: u32,
    column: u32,
    loc: Loc,
    tokens: []Token = undefined,
    diagnostics: *Diagnostics = undefined,

    pub fn init(allocator: std.mem.Allocator, diagnostics: *Diagnostics, source: []const u8) Self {
        return Self{
            .allocator = allocator,
            .source = source,
            .offset = 0,
            .line = 1,
            .column = 1,
            .loc = Loc.init(),
            .diagnostics = diagnostics,
        };
    }

    pub fn deinit(self: *Self) void {
        self.allocator.free(self.tokens);
    }

    pub fn lex(self: *Self) !void {
        var result = std.ArrayList(Token).init(self.allocator);
        while (true) {
            switch (try self.next()) {
                .eof => {
                    self.tokens = try result.toOwnedSlice();
                    return;
                },
                .token => |token| try result.append(token),
            }
        }
    }

    pub fn dump(self: *Self, out: std.fs.File.Writer) !void {
        const lexer_utils = @import("utils/lexer_utils.zig");
        try lexer_utils.dump(self, out);
    }

    fn next(self: *Self) !Result {
        const start = self.offset;
        if (!self.hasMore())
            return .eof;

        const tag = try self.nextToken();
        const end = self.offset;

        std.debug.assert(end > start); // tokens should never be empty!

        const tok = Token{
            .tag = tag,
            .loc = Loc{
                .start = start,
                .end = end,
                .line = self.loc.line,
                .column = self.loc.column,
            },
            .text = self.source[start..end],
        };

        self.loc = Loc{
            .start = start,
            .end = end,
            .line = self.line,
            .column = self.column,
        };

        return Result{ .token = tok };
    }

    fn nextToken(self: *Self) !Token.Tag {
        std.debug.assert(self.hasMore());

        const c = self.consume();
        switch (c) {
            ' ', '\r', '\n', '\t' => return self.lexWhitespace(),
            '+' => return Token.Tag.plus,
            '-' => return Token.Tag.minus,
            '*' => return Token.Tag.star,
            '/' => return try self.lexSlashOrComment(),
            '(' => return Token.Tag.left_paren,
            ')' => return Token.Tag.right_paren,
            '{' => return Token.Tag.left_brace,
            '}' => return Token.Tag.right_brace,
            '.' => return Token.Tag.period,
            ',' => return Token.Tag.comma,
            ':' => return Token.Tag.colon,
            ';' => return Token.Tag.semicolon,
            '|', '&' => return self.lexLogical(c),
            '=', '!', '<', '>' => return self.lexComparison(c),
            '0'...'9' => return self.lexNumber(c),
            else => return Token.Tag.invalid,
        }
    }

    fn lexWhitespace(self: *Self) Token.Tag {
        while (true) {
            const c = self.peek(0) orelse break;
            switch (c) {
                ' ', '\r', '\n', '\t' => _ = self.consume(),
                else => break,
            }
        }
        return Token.Tag.whitespace;
    }

    fn lexSlashOrComment(self: *Self) !Token.Tag {
        if (self.peek(0)) |c| {
            switch (c) {
                '/' => return self.lexCppComment(),
                '*' => return try self.lexCComment(),
                else => return Token.Tag.slash,
            }
        }
        return Token.Tag.slash;
    }

    fn lexCppComment(self: *Self) Token.Tag {
        while (true) {
            const c = self.peek(0) orelse break;
            if (c != '\n') {
                _ = self.consume();
            } else break;
        }
        return Token.Tag.comment;
    }

    fn lexCComment(self: *Self) !Token.Tag {
        var commentStack: i32 = 1;
        _ = self.consume(); // Eat the * after the /
        while (true) {
            // No support (yet) for nested comments: we stop at the first */
            const c1 = self.tryConsume() orelse break;
            const c2 = self.peek(0) orelse break;
            if (c1 == '/' and c2 == '*') commentStack += 1;
            if (c1 == '*' and c2 == '/') {
                _ = self.consume(); // Eat the / after the *
                commentStack -= 1;
                if (commentStack == 0)
                    return Token.Tag.comment;
            }
        }

        try self.diagnostics.append_err(self.loc, "Unterminated comment", .{});
        return Token.Tag.comment;
    }

    fn lexNumber(self: *Self, initial_char: u8) !Token.Tag {
        const peeked = self.peek(0) orelse return Token.Tag.number_literal;
        if (initial_char == '0') {
            _ = self.consume();
            // look for x specifier
            if (peeked == 'x' or peeked == 'X') {
                if (!self.hasMore()) {
                    try self.diagnostics.append_err(self.loc, "Unterminated hexadecimal number", .{});
                    return Token.Tag.invalid;
                }

                return self.lexDecOrHexNumber(true);
            }
        }

        return self.lexDecOrHexNumber(false);
    }

    fn lexDecOrHexNumber(self: *Self, isHex: bool) Token.Tag {
        var isFloat = false;
        var afterExp = false;
        var afterExpSign = false;
        while (true) {
            switch (self.peek(0) orelse return Token.Tag.number_literal) {
                '0'...'9', '_' => _ = self.consume(), // we allow _ in the middle of numbers
                'e', 'E' => {
                    if (isHex) {
                        _ = self.consume();
                    } else if (!afterExp) {
                        _ = self.consume();
                        afterExp = true;
                    } else break;
                },
                'p', 'P' => {
                    if (!afterExp and isHex) {
                        _ = self.consume();
                        afterExp = true;
                    } else break;
                },
                '+', '-' => {
                    if (afterExp and !afterExpSign) {
                        afterExpSign = true;
                        _ = self.consume();
                    } else break;
                },
                'a'...'d', 'f', 'A'...'D', 'F' => {
                    if (isHex) {
                        _ = self.consume();
                    } else break;
                },
                '.' => {
                    if (!isFloat) {
                        isFloat = true; // first dot
                        _ = self.consume();
                    } else break;
                },
                else => break,
            }
        }

        return Token.Tag.number_literal;
    }

    fn lexLogical(self: *Self, initial_char: u8) !Token.Tag {
        const isPipe = initial_char == '|';
        const peeked = self.peek(0) orelse 0;
        if (peeked == initial_char) {
            _ = self.consume();
            return if (isPipe) Token.Tag.pipe_pipe else Token.Tag.ampersand_ampersand;
        } else {
            return if (isPipe) Token.Tag.pipe else Token.Tag.ampersand;
        }
    }

    fn lexComparison(self: *Self, initial_char: u8) !Token.Tag {
        const orEqual = self.peek(0) == '=';
        if (orEqual) _ = self.consume();
        return switch (initial_char) {
            '=' => if (orEqual) Token.Tag.equal_equal else Token.Tag.equal,
            '!' => if (orEqual) Token.Tag.bang_equal else Token.Tag.bang,
            '<' => if (orEqual) Token.Tag.lower_equal else Token.Tag.lower,
            '>' => if (orEqual) Token.Tag.greater_equal else Token.Tag.greater,
            else => unreachable,
        };
    }

    inline fn hasMore(self: Self) bool {
        return self.offset < self.source.len;
    }

    inline fn peek(self: Self, n: usize) ?u8 {
        return if (self.offset + n >= self.source.len) null else self.source[self.offset + n];
    }

    inline fn tryConsume(self: *Self) ?u8 {
        return if (self.hasMore()) self.consume() else null;
    }

    // Beware: this panics if we are at the end of the source
    inline fn consume(self: *Self) u8 {
        const cc = self.source[self.offset];
        self.offset += 1;
        if (cc == '\n') {
            self.line += 1;
            self.column = 1;
        } else self.column += 1;

        return cc;
    }
};
