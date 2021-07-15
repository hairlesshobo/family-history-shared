- CSD: show drive summary
- CSD: show archive summary
- CSD: Restore entire drive
- CSD: Verify drive
- CSD: Delete files that have been removed from source
- CSD: Scan only for changes
- CSD: Make it so that "clean" also removes files from index that do not exist on disk
- CSD: Add handling for modified files (size or date change)

- Tape: Restore entire tape
- Tape: Add scan rate to `file scanner`
- Tape: Add scan rate to `Sizer`

- Disc: Restore entire disc
- Disc: Add scan rate to `file scanner`
- Disc: Add scan rate to `Sizer` and `Distributor`
- Disc: Add scan rate to `Distributor`
- Disc: Port file scanner performance improvements
- Disc: Port sizer and distributor performance improvements


## future
- Selectvely restore files
- Search entire archive
- Add handling for inline color:
```
`r_ (red)
`b_ (blue)
`g_ (green)
`c_ (cyan)
`m_ (magenta)
`y_ (yellow)
`w_ (white)
`G_ (gray)

`dr_ (dark red)
`db_ (dark blue)
`dg_ (dark green)
`dc_ (dark cyan)
`dm_ (dark magenta)
`dy_ (dark yellow)
`dw_ (dark white)
`dG_ (dark gray)

`n (normal / default / reset)