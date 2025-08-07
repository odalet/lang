use itertools::Itertools;

fn main() -> color_eyre::Result<()> {
    color_eyre::install()?;

    let input = include_str!("input.txt");

    // Part 1
    let max = input
        .lines()
        .map(|v| v.parse::<u64>().ok())
        .coalesce(|a, b| match (a, b) {
            (None, None) => Ok(None),
            (None, Some(b)) => Ok(Some(b)),
            (Some(a), Some(b)) => Ok(Some(a + b)),
            (Some(a), None) => Err((Some(a), None)),
        })
        .max()
        .flatten()
        .unwrap_or_default();

    println!("Max: {max:?}");

    // Part 2
    let answer = input
        .lines()
        .map(|v| v.parse::<u64>().ok())
        .coalesce(|a, b| match (a, b) {
            (None, None) => Ok(None),
            (None, Some(b)) => Ok(Some(b)),
            (Some(a), Some(b)) => Ok(Some(a + b)),
            (Some(a), None) => Err((Some(a), None)),
        })
        .flatten()
        .sorted_by_key(|&v| std::cmp::Reverse(v))
        .take(3)
        .sum::<u64>();

    println!("Answer: {answer:?}");

    Ok(())
}
