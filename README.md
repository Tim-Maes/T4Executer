[![Build Status](https://dev.azure.com/Epsicode/T4Executer/_apis/build/status/Epsilekt.T4Executer?branchName=master)](https://dev.azure.com/Epsicode/T4Executer/_build/latest?definitionId=1&branchName=master)
# T4Executer

Fully configurable extension for Visual Studio 2017 & 2019
Transform T4 Templates when building your solution. 

## Usage

Just install the extension. You can download it at [the marketplace](https://marketplace.visualstudio.com/items?itemName=TimMaes.ttexecuter) or install it in Visual Studio.

## Configuration

Enable or disable T4Executer via `Extensions - T4Executer - Enable/Disable`, it's enabled by default.
T4Executer will run all your templates when building your solution.

Run all T4 Templates in your solution by clicking the T4 icon on the solution explorer menu.

![menuItem](https://i.ibb.co/bQ90BwH/menuItem.png)

You can set which T4 Templates to execute before build, after build or which Templates to ignore completely on build time via `Extensions - T4Executer - Configure`.

Specify to preserve generated file timestamp when the content is not changed via `Extensions - T4Executer - Preserve timestamp`. This will cause only templates with new changes to be transformed.
