# TSMerge
A program that merges TS files by directly analyzing and removing overlapping binary data.

## Usage

```
Description:
  Merge MPEG-TS files into one

Usage:
  TSMerge <OUTPUT> [options]

Arguments:
  <OUTPUT>  File output name

Options:
  -i <FILE> (REQUIRED)  Add inputs
  -p <SIZE>             Pattern size to search (in MB) [default: 1]
  --version             Show version information
  -?, -h, --help        Show help and usage information
```

## Sample

```
TSMerge -i A.ts -i B.ts -i C.ts MERGE.ts
```

## Flowchart

```mermaid
gantt
    dateFormat  YYYY-MM-DD
    axisFormat  -

    section A.ts
    A.ts: 2024-11-01, 8d

    section B.ts
    B.ts: 2024-11-08, 7d

    section C.ts
    C.ts: 2024-11-13, 7d

    section MERGE.ts
    MERGE.ts: 2024-11-01, 19d


```
