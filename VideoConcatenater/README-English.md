# Video Concatenater
Concatenate video pieces  
视频合成器: 将多个视频片段合并  

[简体中文](https://github.com/Keqing-Yuheng/VideoConcatenater/blob/main/README-English.md "README.md") | **English**

## Instructions

1. Prepare the video pieces you have get from somewhere
2. Put them in a folder (The folder should not include other files, but this may be changed in later versions)
3. Copy the path of the folder mentioned above, which can be absolute path or relative path
4. Fill the blanks in this program, which contain the input folder (the one you have just copied) and the output file
5. Click the "Run" button and wait until it finishes

## Option Description

Checkbox "Only Generate List": Only generate "List.txt", which is a list iof files in the targeted direction and will be used by FFmpeg. The program will not call FFmpeg when the checkbox is checked.

## Working Method and Attenrion

The program will get the files in the targeted direction and sort it. Then the program will generate a file list as the input of FFmpeg. Ultimately, the program will call FFmpeg and concatenate the video pieces using "concat" method

The advantage of concat is the loseless result as well as the speed. However, using concat, you ought to ensure that the input video pieces are encoded in the same codec. Given that the developing purpose is to concatenate a group of video pieces conveniently when m3u8 file is not provided, we will not consider how to concatenate video pieces with different codec in early versions