const std = @import("std");

pub const Loc = struct {
    const Self = @This();

    line: u32, // Starts @ 1
    column: u32, // Starts @ 1
    start: usize,
    end: usize,

    pub fn init() Self {
        return Self{ .line = 1, .column = 1, .start = undefined, .end = undefined };
    }

    pub fn format(self: Self, comptime fmt: []const u8, options: std.fmt.FormatOptions, writer: anytype) !void {
        _ = fmt;
        _ = options;
        try writer.print("{d}:{d}", .{ self.line, self.column });
    }
};
