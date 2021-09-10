## Linux support:
----------------------------------------------------------------
* Finish tape driver
* Add mappings for unc paths to other network paths
    * Add source path config validation
    * Add support for smb network paths
    * Add support for nfs network paths
    * Add support for rsync network paths
* Add CSD support
* Add ISO creation support
* Add disc burn support
* Add disc verification support
* Explore the idea of bundling archiver into an `appimage` file

## Windows support:
----------------------------------------------------------------
* Natively determine disc size
* Add WindowsNativeStreamReader
* Add Native ISO creation
* Add disc burning support


## Global Functionality:
----------------------------------------------------------------
* Add ability to detect if drive is BD capable (both read and write)
* keep track of copy speed performance per tape and csd, use that in future calculations
* Add a per-user config to the user's home directory
* Add a wizard mode for other users
* Search entire archive, across media types
* Finish config validation
* Port archiver to new config binding
* Add optical drive model to select drive list
* Eliminate `DiscGlobals.cs`
* Eliminate `CsdGlobals.cs`
* Eliminate `Globals.cs`
* Rename `GuiGlobals` to `GuiStatic`
* Improve documentation in config files

## CSD Functionality:
----------------------------------------------------------------
* Show drive summary
* Show archive summary
* Restore entire drive
* Verify drive
* Delete files that have been removed from source
* Scan only for changes
* Make it so that "clean" also removes files from index that do not exist on disk
* Add handling for modified files (size or date change)


## Tape Functionality:
----------------------------------------------------------------
* Restore entire tape
* Add scan rate to `file scanner`
* Add scan rate to `Sizer`
* Add ETA to tape display
* Factor in long file names when calculating total archive size. long file names require two headers
* Add "last run" to tape selection list


## Disc Functionality
----------------------------------------------------------------
* Restore entire disc
* Add ability to restore entire tape, requiring to specify new restore directory
* Add ability to restore one or more discs, requiring to specify new restory directory
* Add scan rate to `file scanner`
* Add scan rate to `Sizer` and `Distributor`
* Add scan rate to `Distributor`
* Port file scanner performance improvements
* Port sizer and distributor performance improvements



## Useful links
----------------------------------------------------------------
* https://www.commandlinefu.com/commands/view/13582/backup-to-lto-tape-with-progress-checksums-and-buffering
* https://www.codeproject.com/Articles/15487/Magnetic-Tape-Data-Storage-Part-Tape-Drive-IO-Co
* https://github.com/icsharpcode/SharpZipLib/blob/master/src/ICSharpCode.SharpZipLib/Tar/TarBuffer.cs
* https://stackoverflow.com/questions/8221136/fifo-queue-buffer-specialising-in-byte-streams



## Future ideas
----------------------------------------------------------------
* Add handling for inline color:
    * `r_` (red)
    * `b_` (blue)
    * `g_` (green)
    * `c_` (cyan)
    * `m_` (magenta)
    * `y_` (yellow)
    * `w_` (white)
    * `a_` (gray)
    * `dr_` (dark red)
    * `db_` (dark blue)
    * `dg_` (dark green)
    * `dc_` (dark cyan)
    * `dm_` (dark magenta)
    * `dy_` (dark yellow)
    * `dw_` (dark white)
    * `da_` (dark gray)
    * `n_` (normal / default / reset)


## Linux Notes
----------------------------------------------------------------
* Get optical drive details from /proc/sys/dev/cdrom/info
* may be able to inspect cd-drive source code to see how it pulls cd drive list:
    * https://git.savannah.gnu.org/cgit/libcdio.git/tree/src