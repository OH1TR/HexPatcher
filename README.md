# Patch binary files with search and replace

Create config file with lines:

F:00 11 22

R:33 44 55

And run:

HexPatcher.exe -i input.bin -o output.bin -p configfile.txt

Program reads input.bin then replaces all instances of 00 11 22 with 33 44 55. And writes resut to output.bin.
