# GoPro Ordering

This is a C# Application that parses a directory with GoPro content and adds a numerical prefix that allows windows to sort them accurately.

# Download
Every update is compiled for Windows already. You can download them here: https://github.com/Vaei/GoProOrdering/tree/master/Binaries

# Correct Usagse
This is only for GoPro Hero 8, 9, 10. Older versions of GoPro use a different file format. [You can see GoPro's naming convention for yourself here](https://community.gopro.com/s/article/GoPro-Camera-File-Naming-Convention?language=en_US)

Does not support looping video - single video and chaptered video only.

There should be nothing except for your GoPro files in the directory.

There is no undo, so if you have a lot of files and you don't want to possibility of error, back them up first.

# Example Usage
This example only shows real-world usage because it is a very simply application. Normally all you need to do is set the directory and click the button.

In this example I used two 256GB SD cards during a long road trip, the first SD card was already processed and had a prefix up to 053.

Here is the result of the already parsed first SD card:

![](https://github.com/Vaei/GoProOrdering/blob/master/GitPage/preview_004.png)

The second SD card starts with a prefix of 54 for coherent ordering. Adding 1 extra padding because the files only go into double digits, whereas the first sd card went into triple; not required, but cleaner.

![](https://github.com/Vaei/GoProOrdering/blob/master/GitPage/preview_001.png)

It will show a list of all the changes it will make:

![](https://github.com/Vaei/GoProOrdering/blob/master/GitPage/preview_002.png)

When you click accept, it will complete the renaming:

![](https://github.com/Vaei/GoProOrdering/blob/master/GitPage/preview_003.png)