# Gherkin Editor

An editor for specification by example in Gherkin.
It is a WPF application in C# and is developed by using Visual studio 2015.

# Screen shot
![sceen shot](https://github.com/bzquan/GherkinEditor/blob/master/ScreenShot.png)

# Features
1. Keywords highlighting
2. Folding scenarios and tables
3. Pretty formatting Gherkin features dynamically
4. Formatting Gherkin table dynamically
5. Embedding images to Gherkin document
6. Support mathematical formula in LaTex style
7. Supporting globalization
8. Generation of executable specification in C++ based on [GoogleTest](https://github.com/google/googletest).

# Building
Please select "Net40" solution platforms when buiding by using Visual studio 2015 because AvalonEdit project uses conditional compiling.  
Solution Platforms : Net40

# Note
1. [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) is included. It is customized for search & replace, embedded images etc.
2. [Gherkin Parser](https://github.com/cucumber/cucumber/tree/master/gherkin) is included. It is customized for generating executable specification in C++.
3. [WPF-Math](https://github.com/ForNeVeR/wpf-math) is used to support mathematical formula.

