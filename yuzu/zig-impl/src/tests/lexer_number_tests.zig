const std = @import("std");
const Token = @import("yuzu").token.Token;
const testLexer = @import("utils.zig").testLexer;

test "Valid Decimal Integers" {
    try std.testing.expectEqual(0, try testLexer("0", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("2", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("3", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("4", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("5", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("6", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("7", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("8", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("9", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("42", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1234567890123456789012345678901234567890", &.{Token.Tag.number_literal}));
}
test "Valid Hexadecimal Integers" {
    try std.testing.expectEqual(0, try testLexer("0x0", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x01", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x10", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1234567890abcdefABCDEF", &.{Token.Tag.number_literal}));
}
test "Valid Decimal Floats" {
    try std.testing.expectEqual(0, try testLexer("3.14", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("3.1415926535898", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1e", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1e0", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1.e0", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1E1", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1E-1", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("1E+1", &.{Token.Tag.number_literal}));
}
test "Valid Hexadecimal Floats" {
    try std.testing.expectEqual(0, try testLexer("0x3.14", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x3.1415926535898", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1p", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1p0", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1P1", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1P-1", &.{Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("0x1P+abcd", &.{Token.Tag.number_literal}));
}
test "Invalid Numbers" {
    try std.testing.expectEqual(0, try testLexer("3.14.12", &.{Token.Tag.number_literal, Token.Tag.period, Token.Tag.number_literal}));
    try std.testing.expectEqual(0, try testLexer("3.14+", &.{Token.Tag.number_literal, Token.Tag.plus}));
    try std.testing.expectEqual(0, try testLexer("3.14-", &.{Token.Tag.number_literal, Token.Tag.minus}));
    try std.testing.expectEqual(0, try testLexer("3.14-", &.{Token.Tag.number_literal, Token.Tag.minus}));
}
