const std = @import("std");
const Token = @import("yuzu").token.Token;
const testLexer = @import("utils.zig").testLexer;

test "C++ Comments" {
    try std.testing.expectEqual(0, try testLexer("// simple comment", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer("// simple comment with weird characters: <世界! 🚀", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer(
        \\// simple comment 1
        \\// simple comment 2
    , &.{ Token.Tag.comment, Token.Tag.whitespace, Token.Tag.comment }));
}
test "C Comments" {
    try std.testing.expectEqual(0, try testLexer("/* single line comment */", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer("/* single /* nested line */ comment */", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer("/* single /*/ comment */", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer("/* comment with weird characters: <世界! 🚀 */", &.{Token.Tag.comment}));
    try std.testing.expectEqual(0, try testLexer(
        \\/* simple comment 1
        \\   simple comment 2 */
    , &.{Token.Tag.comment}));
}
test "Unterminated C Comments" {
    try std.testing.expectEqual(1, try testLexer("/* unterminated", &.{Token.Tag.comment}));
    try std.testing.expectEqual(1, try testLexer("/* /* nested */ unterminated", &.{Token.Tag.comment}));
}
