<h1 align="center">
  <img src="https://user-images.githubusercontent.com/10968691/125787499-7da697e2-5f7f-4d83-a9b0-995bbd23d032.png">
</h1>

RSIEdit is a GUI application for creating and editing [RSI files](https://hackmd.io/@ss14/rsis) and converting existing DMI files to the RSI format. 

It replaces the old [RSI Editor](https://github.com/space-wizards/RSI-editor) with this project and [RSI.py](https://github.com/space-wizards/RSI.py) with [RSI.NET](https://github.com/space-wizards/RSI.NET).


## Installing
The latest release for your OS can be downloaded [here](https://github.com/space-wizards/RSIEdit/releases/latest).

You can find every previous release [here](https://github.com/space-wizards/RSIEdit/releases).

### Arch Linux
You can find an (unofficial) installation package on the [AUR](https://aur.archlinux.org/packages/rsiedit-bin/):
```
paru -S rsiedit-bin
```
or
```
yay -S rsiedit-bin
```
Note that this package is not maintained by us. Read the PKGBUILD and then proceed at your own risk.

## Building
1. Clone this repo.
2. Run `git submodule update --init` to init submodules and download the importer.
3. Compile the solution.

## Repository asset licenses
When a GitHub url ending in .dmi is pasted into the editor it will try to import it.  
The json at [Assets/repo-licenses](https://github.com/space-wizards/RSIEdit/Assets/repo-licenses.json) dictates which license it will use by default for that repository.  
Matches closest to the top take preference.  
This license is not guaranteed to be correct and should always be checked manually after importing, as some repositories use different licenses depending on which folder they are in, or the commit may specifically attribute a different license to that particular asset.  
The editor will always try to fetch this file from the repository to read up-to-date repository and license pairings, but will fallback to the local copy if it is unable to.  
These licenses should be one of the valid ones listed in [rsi-schema.json](https://github.com/space-wizards/space-station-14/blob/master/.github/rsi-schema.json)
