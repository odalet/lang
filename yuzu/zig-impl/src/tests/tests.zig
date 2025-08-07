const Token = @import("yuzu").token.Token;

// Fake tests, ensures we can reference the yuzu module
const std = @import("std");
test "T1" {
    try std.testing.expectEqual(1, 1);
}
test "T2" {
    try std.testing.expectEqual(2, 2);
}

// Include all tests
comptime {
    _ = @import("lexer_comment_tests.zig");
    _ = @import("lexer_number_tests.zig");
}