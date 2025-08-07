const Loc = @import("loc.zig").Loc;

pub const Token = struct {
    text: []const u8,
    tag: Tag,
    loc: Loc,
    // is_valid: bool,

    pub const Tag = enum {
        invalid,
        eof,
        whitespace,
        comment, // // ..., or /* ... */
        plus, // +
        minus, // -
        star, // *
        slash, // /
        left_paren, // (
        right_paren, // )
        left_brace, // {
        right_brace, // }
        period, // .
        comma, // ,
        colon, // :
        semicolon, // ;
        ampersand, // &
        ampersand_ampersand, // &&
        pipe, // |
        pipe_pipe, // ||
        equal, // =
        equal_equal, // ==
        lower, // <
        lower_equal, // <=
        greater, // >
        greater_equal, // >=
        bang, // !
        bang_equal, // !=
        number_literal
        // identifier,
    };
};
