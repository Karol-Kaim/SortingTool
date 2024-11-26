There are two parts of this solution:
- SampleFileGenerationApp - it generates a file based on the dictionary (sample dictionaries are in folder SampleFiles)
- SortingTool - it sorts the input file and generate the resulted sorted file
- There are two configuration in the solution that allows to either run one or another project
- handling arguments is not fully implemented, hence some configuration is hard-coded in program.cs file

Please note that this code is not production ready.
There are still multiple ways that it can be improved:
- adding handling arguments and/or configuration
- exception handling
- introducing IoC container (and using IoC)
- code clean-up
- logging - currently there are some diagnostic information in #DEBUG written to the console
