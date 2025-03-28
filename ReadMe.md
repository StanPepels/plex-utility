Welcome to the Media Util this hacked together little program can be used to convert ripped .iso files or blu-ray folders to .mkv files

Use at your own risk. Any harm done to files or content is your own responsibility. The author is not liable for any damage to or loss of data / files etc stored on you pc.

### Dependencies
The program depends on MakeMkv so make sure it's installed. It can be downloaded from here: [MakeMKV](https://www.makemkv.com/)
Additionally to use the plex metadata editor VLC media player is required. It can be downloaded from here [VLC Media player](https://www.videolan.org/index.nl.html)

### Basic Usage (converter)
1. Provide an input folder. If left empty the current working directory is used. this is the folder that will be scanned for input files and folders
2. Provide an output foder. If left empty the current working directory is used. This is where all the converted files will be placed.
3. Provide an encode folder.  If left empty the current working directory is used. This is where the encoded output will be placed.
4. Select a presets file this is exported using the handbrake gui.
5. Select the preset to be used for encoding. 
6. Press the `Scan` button. this will scan the input folder and list them in the GUI. The scan will pick up any `.iso` files and any folders containing a `BDMV` folder. 
Note that this does not search subfolders. Additionally this will scan all found items for titles and calculate the required disk space
7. Remove any entries you do not whish to convert. or modify the titles to convert.
8. Press the `Convert` button. This will start the conversion process.
9. (Optional) Press `Cancel` to terminate the running conversion and stop converting.

### Basic Usage (plex metadata)
1. Provide an input folder.
2. Press scan
3. Fill in the fields for title, year and imdb tag. The imdb tag can be found in the url when on imdb
    For example: `https://www.imdb.com/title/tt0311289/?ref_=nv_sr_srsg_0_tt_6_nm_2_in_0_q_holes` the tag is *tt0311289*
4. Press the save icon or the save all button to rename the folder to the correct fomat
5. If any itms show the warning symbol. press the modify button
6. Configure all the files appropriatly
7. Press apply to make the changes. This will name all files correctly and put them into the appropriate folder(s)

### Aditional Options
- **Skip Existing:** If a folder the out folder for an item already exists the item will be skipped.

### NOTE
- In case a dependency cannot be found you can manually specify the install locations via Options -> preferences.