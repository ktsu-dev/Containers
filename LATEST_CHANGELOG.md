## v2.0.4 (patch)

Changes since v2.0.3:

- Validate the delay before writing in DelayLine.Process [patch] (Claude)
- Grow ContiguousSet before touching its index instead of rolling back (Claude)
- Clamp contiguous container growth to Array.MaxLength [patch] (Claude)
- Reject ring buffer sizes above 2^30 with ArgumentOutOfRangeException [patch] (Claude)

