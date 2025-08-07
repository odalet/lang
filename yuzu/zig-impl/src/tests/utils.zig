const std = @import("std");
const Lexer = @import("yuzu").lexer.Lexer;
const Token = @import("yuzu").token.Token;
const Diagnostics = @import("yuzu").diagnostics.Diagnostics;

// Returns the number of diagnostics
pub fn testLexer(source: [:0]const u8, expected_token_tags: []const Token.Tag) !usize {
    const allocator = std.testing.allocator;

    var diagnostics = Diagnostics.init(allocator);
    defer diagnostics.deinit();

    var lexer = Lexer.init(allocator, &diagnostics, source);
    defer lexer.deinit();

    try lexer.lex();

    std.testing.expectEqual(expected_token_tags.len, lexer.tokens.len) catch |err| {
        std.debug.print("Expected tokens count != actual.", .{});
        std.debug.print("\n\tExpected: ", .{});
        for (expected_token_tags) |tag| {
            std.debug.print("{s}, ", .{@tagName(tag)});
        }
        std.debug.print("\n\tActual  : ", .{});
        for (lexer.tokens) |tok| {
            std.debug.print("{s}, ", .{@tagName(tok.tag)});
        }

        std.debug.print("\n", .{});
        return err;
    };

    for (expected_token_tags, lexer.tokens) |expected, tok| {
        const actual = tok.tag;
        try std.testing.expectEqual(expected, actual);
    }

    return diagnostics.list.items.len;
}
