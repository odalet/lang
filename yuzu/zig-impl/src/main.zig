const std = @import("std");
const builtin = @import("builtin");
const Diagnostics = @import("yuzu").diagnostics.Diagnostics;
const Lexer = @import("yuzu").lexer.Lexer;
// const Diagnostics = @import("diagnostics.zig").Diagnostics;
// const Lexer = @import("lexer.zig").Lexer;

const version = std.SemanticVersion{ .major = 0, .minor = 0, .patch = 1 };

var gpa = std.heap.GeneralPurposeAllocator(.{}){};
var allocator: std.mem.Allocator = gpa.allocator();

pub fn main() !void {
    const stdout_file = std.io.getStdOut().writer();
    try about(stdout_file);

    // const source_code =
    //     \\/* single line block comment */
    //     \\/* multi-line
    //     \\   block comment */
    //     \\// single line comment
    //     \\fun foo() {
    //     \\  val h = 0xabcd;
    //     \\  val i = 42;
    //     \\  val f = 3.14;
    //     \\  return 1 + 1 - 42;
    //     \\} /* ending with a comment with weird characters: <世界! 🚀>"
    // ;

    const source_code = "& && | || &&& |||";

    var diagnostics = Diagnostics.init(allocator);
    defer diagnostics.deinit();

    var lexer = Lexer.init(allocator, &diagnostics, source_code);
    defer lexer.deinit();

    try lexer.lex();

    // const seq = try lexer.lex(allocator, source_code);
    // defer allocator.free(seq);

    try stdout_file.print("Diagnostics:\n", .{});
    for (diagnostics.list.items) |diag| {
        try stdout_file.print("{}\n", .{diag});
    }

    try stdout_file.print("Tokens:\n", .{});
    try lexer.dump(stdout_file);
    // for (lexer.tokens) |tok| {
    //     try stdout_file.print("L{d}C{d} - {d}:{d} [{s}] = '{s}'\n", .{
    //         tok.loc.line,
    //         tok.loc.column,
    //         tok.loc.start,
    //         tok.loc.end,
    //         @tagName(tok.tag),
    //         tok.text,
    //     });
    // }
}

fn about(out: std.fs.File.Writer) !void {
    try out.print("\nYuzu Compiler version {}\n", .{version});
    try out.print("Built with Zig {}\n", .{builtin.zig_version});
}
