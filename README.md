# Video Concatenater
Concatenate video pieces  
视频合成器: 将多个视频片段合并  

**简体中文** | [English](https://github.com/Keqing-Yuheng/VideoConcatenater/blob/main/README-English.md "README-English.md")

## 使用方法

1. 准备好你想要合并的视频片段
2. 将它们放在一个文件夹内 (文件夹内不能有其他文件, 可能会在后续版本修改)
3. 复制视频所在目录的路径, 绝对路径或相对路径均可
4. 将输入文件夹路径和输出文件路径填入程序
5. 单击 "运行" 按钮, 等待程序自动完成

## 选项说明

"仅生成列表 (不执行合并)" 复选框: 仅生成供FFmpeg使用的文件列表List.txt, 而不启动FFmpeg

## 原理与注意事项

本程序获取目标目录的文件并进行排序, 生成一个文件列表, 然后传入FFmpeg以使用concat方式合并

使用concat方式合并优点是无损且速度快, 但必须保证输入视频编码相同. 鉴于本程序的开发目的是在缺失m3u8文件的情况下便捷合并分段视频, 在程序的早期版本我们并不会考虑合并编码不同的视频