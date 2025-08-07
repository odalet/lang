const std = @import("std");
const Lexer = @import("../lexer.zig").Lexer;
const Token = @import("../token.zig").Token;
const Diagnostics = @import("../diagnostics.zig").Diagnostics;

pub fn dump(lexer: *Lexer, out: std.fs.File.Writer) !void {
    for (lexer.tokens) |tok| {
        var list = std.ArrayList(u8).init(lexer.allocator);
        defer list.deinit();

        for (tok.text) |c| {
            switch (c) {
                '\r' => try list.appendSlice("\\r"),
                '\n' => try list.appendSlice("\\n"),
                '\t' => try list.appendSlice("\\t"),
                else => try list.append(c),
            }
        }

        try out.print("{} ({d} -> {d}) [{s}] = <{s}>\n", .{
            tok.loc,
            tok.loc.start,
            tok.loc.end,
            @tagName(tok.tag),
            list.items,
        });
    }
}
