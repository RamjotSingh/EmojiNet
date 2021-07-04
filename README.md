# EmojiNet
Helpers to make it easy for detecting emojis in .Net

## Motivation

As part of a project I needed to detected if a provided unicode scalar corresponded to an emoji. After searching I didn't find an up-to-date way to do the same. I also realized that is was not easy to keep adding every code by hand as the list keeps on growing. Hence I automated the process of going through unicode.org and find all scalars which correspond to emoji.
