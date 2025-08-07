const std = @import("std");
const Loc = @import("loc.zig").Loc;

pub const Severity = enum { err, wrn, msg };

pub const Diagnostic = struct {
    const Self = @This();

    severity: Severity,
    loc: Loc,
    message: []const u8,

    pub fn new(sev: Severity, loc: Loc, message: []const u8) Diagnostic {
        return Self{
            .severity = sev,
            .loc = Loc{ .line = loc.line, .column = loc.column, .start = loc.start, .end = loc.end }, // TODO: can we just pass loc?
            .message = message,
        };
    }

    pub fn format(value: Self, comptime fmt: []const u8, options: std.fmt.FormatOptions, writer: anytype) !void {
        _ = fmt;
        _ = options;
        try writer.print("{}: {s}: {s}", .{
            value.loc,
            severity_to_text(value.severity, false),
            value.message,
        });
    }

    fn severity_to_text(sev: Severity, short_label: bool) []const u8 {
        return switch (sev) {
            .err => if (short_label) "E" else "Error",
            .wrn => if (short_label) "W" else "Warning",
            .msg => if (short_label) "M" else "Message",
        };
    }
};

pub const Diagnostics = struct {
    const Self = @This();

    arena: std.heap.ArenaAllocator,
    list: std.ArrayList(Diagnostic),

    pub fn init(allocator: std.mem.Allocator) Self {
        return Self{
            .arena = std.heap.ArenaAllocator.init(allocator),
            .list = std.ArrayList(Diagnostic).init(allocator),
        };
    }

    pub fn deinit(self: *Self) void {
        self.list.deinit();
        self.arena.deinit();
    }

    pub fn append_err(self: *Self, loc: Loc, comptime fmt: []const u8, args: anytype) !void {
        try self.append(Severity.err, loc, fmt, args);
    }

    pub fn append_wrn(self: *Self, loc: Loc, comptime fmt: []const u8, args: anytype) !void {
        try self.append(Severity.wrn, loc, fmt, args);
    }

    pub fn append_msg(self: *Self, loc: Loc, comptime fmt: []const u8, args: anytype) !void {
        try self.append(Severity.msg, loc, fmt, args);
    }

    fn append(self: *Self, sev: Severity, loc: Loc, comptime fmt: []const u8, args: anytype) !void {
        const message = try std.fmt.allocPrint(self.arena.allocator(), fmt, args);
        errdefer self.arena.allocator().free(message);

        const diag = Diagnostic.new(sev, loc, message);
        try self.list.append(diag);
    }
};
