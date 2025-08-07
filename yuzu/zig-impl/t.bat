@echo off
echo ***************************************************************************************
zig test --dep yuzu -Mroot="./src/tests/tests.zig" -Myuzu="./src/yuzu/yuzu.zig" -O Debug --color on --test-runner "./src/tests/runner.zig"