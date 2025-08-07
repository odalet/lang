const std = @import("std");

// Although this function looks imperative, note that its job is to
// declaratively construct a build graph that will be executed by an external
// runner.
pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const yuzu_module = b.addModule("yuzu", .{
        .root_source_file = b.path("src/yuzu/yuzu.zig"),
    });

    const exe = b.addExecutable(.{
        .name = "yuzu",
        .root_source_file = b.path("src/main.zig"),
        .target = target,
        .optimize = optimize,
    });

    exe.root_module.addImport("yuzu", yuzu_module);
    b.installArtifact(exe);

    const run_cmd = b.addRunArtifact(exe);
    run_cmd.step.dependOn(b.getInstallStep());
    if (b.args) |args| run_cmd.addArgs(args);
    const run_step = b.step("run", "Run the app");
    run_step.dependOn(&run_cmd.step);

    const tests = b.addTest(.{
        .root_source_file = b.path("src/tests/tests.zig"),
        .target = target,
        .optimize = optimize,
        .test_runner =  b.path("src/tests/runner.zig"),
    });

    tests.root_module.addImport("yuzu", yuzu_module);
    b.installArtifact(tests); // let's copy the generated test executable to zig-out

    const test_step = b.step("test", "Run all the tests");
    test_step.dependOn(&b.addRunArtifact(tests).step);
}
