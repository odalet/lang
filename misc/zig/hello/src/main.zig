const std = @import("std");
const math = std.math;

pub fn main() !void {
    // Prints to stderr (it's a shortcut based on `std.io.getStdErr()`)
    std.debug.print("All your {s} are belong to us.\n", .{"codebase"});

    // stdout is for the actual output of your application, for example if you
    // are implementing gzip, then only the compressed bytes should be sent to
    // stdout, not any debugging messages.
    const stdout_file = std.io.getStdOut().writer();
    var bw = std.io.bufferedWriter(stdout_file);
    const stdout = bw.writer();

    try stdout.print("Run `zig build test` to run the tests.\n", .{});

// See:
// * https://github.com/ziglang/zig/issues/13431
// * https://github.com/ziglang/zig/issues/1534
// * 
// * https://github.com/ziglang/zig/commit/929cb68fdb088008d9a886fe71d1aff72c2fd52a
// * https://discourse.llvm.org/t/rfc-add-support-for-division-of-large-bitint-builtins-selectiondag-globalisel-clang/60329/2
// * https://reviews.llvm.org/D126644

    const uType = u128; // u160 -> failure

    var frame: uType = 1234567890;
    //var frame: u7 = 0;
    //var frame: u32 = 0;


    // Below: copied from lib/std/fmt.zif

    const base = 10;
    const case = std.fmt.Case.lower;
    const int_value = frame;
    const value_info = @typeInfo(@TypeOf(int_value)).Int;
    const abs_value = math.absCast(int_value);
    const min_int_bits = comptime math.max(value_info.bits, 8);
    const MinInt = std.meta.Int(.unsigned, min_int_bits);

    var buf: [1 + math.max(value_info.bits, 1)]u8 = undefined;
    var a: MinInt = abs_value;
    var index: usize = buf.len;
    while (true) {
        const digit = a % base;
        index -= 1;
        buf[index] = std.fmt.digitToChar(@intCast(u8, digit), case);
        a /= base;
        if (a == 0) break;
    }

    try stdout.print("\n", .{});
    try stdout.print("-----------------------------------------------\n", .{});
    try stdout.print("value : {}\n", .{frame});
    try stdout.print("info  : {}\n", .{value_info});
    try stdout.print("buffer: {s}\n", .{buf});

    try bw.flush(); // don't forget to flush!
}

test "simple test" {
    var list = std.ArrayList(i32).init(std.testing.allocator);
    defer list.deinit(); // try commenting this out and see if zig detects the memory leak!
    try list.append(42);
    try std.testing.expectEqual(@as(i32, 42), list.pop());
}
