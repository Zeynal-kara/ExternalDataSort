# File Sort

## Instructions
Your task is to write a program that will be able to sort a file using a limited amount of memory.
The size of the generated file should be at least 1024MB - on a 64-bit machine this means 134217728 
integers. At no point in time your program should use more than 100MB of memory.

### Constraints:
* You have to use the proposed C# skeleton.
* The program should be able to run with a memory constraint of 100MB.
* The integers in the file are not necessarily unique.
* The input file contains only 64-bit integers separated with a newline character. The last integer is followed by also a newline.

### Expectations:
We understand this is a heavily timeboxed task and will therefore not judge you on whether 
your application runs and produces correct output as long as you can explain your reasoning and
tell us what would your next steps be. A 100% completion of the task would contain:
- Code that compiles and runs without errors
- A test suite which proves that your solution is correct.
- Output file has to be same size with Input in terms of bytes, only sorting of the numbers are different.

You should be able to explain what algorithm and data structures are you using and tell us
why do you think your solution is optimal. If you write tests, you should be also able
to present your reasoning on why do you think the tests prove the correctness of your solution.
If there has been any problem that prevented you from finishing this task, you should be able to
tell us how would you investigate the issue you've been having.

## Using this repository
You can generate values with the generator utility for your preferred system. 
By default, with no arguments, it will output exactly 1024MB of integers.
For convenience, we've provided you with a binary for 3 common environemnts:
`generator_linux` for linux, `generator_macos` for MacOS, and `generator.exe` for Windows,
all compiled for the `amd64` (modern 64-bit x86 Intel/AMD) architecture.

To generate the default number of integers: in linux `generator/generator_linux -o output_file` or in windows `generator/generator.exe -o output_file`
To generate 1000 integers: in linux `generator/generator_linux -o output_file 1000` or in windows `generator/generator.exe -o output_file 1000`
To output 1000 integers to terminal: in linux `generator/generator_linux 1000` or in windows `generator/generator.exe output_file`

The generator is written in Go and can be compiled for different architectures of needed.
To compile it, first list the architectures:
```shell
go tool dist list
```
Check if your desired OS and architecture are supported and compile them with e.g.:
```shell
cd src/main/go
GOOS=windows GOARCH=386 go build -o generator_win32.exe
```

There is a binary file for you to compile your code in 100MB limitation. You can find the bridge file `procgov.exe` in the project root folder.
From the command line, you can run test as follows:
- Sort a file (limiting memory to 100MB) in debug mode: `procgov.exe -maxmem 100M .\bin\debug\net6.0\AlgorithmTest.exe <inputFilename> <outputFilename>`
- Sort a file (limiting memory to 100MB) in release mode: `procgov.exe -maxmem 100M .\bin\release\net6.0\AlgorithmTest.exe <inputFilename> <outputFilename>`