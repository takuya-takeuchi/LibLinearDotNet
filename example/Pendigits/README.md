﻿# Pendigits

## How to use?

## 1. Preparation

This sample requires handwritten digits data set for train and test.  
Please download the following files.

* https://www.csie.ntu.edu.tw/~cjlin/libsvmtools/datasets/multiclass/pendigits
* https://www.csie.ntu.edu.tw/~cjlin/libsvmtools/datasets/multiclass/pendigits.t

At last, deploy **pendigits** and **pendigits.t** to &lt;Pendigits_dir&gt;.

## 2. Build

1. Open command prompt and change to &lt;Pendigits_dir&gt;
1. Type the following command
````
dotnet build -c Release
````
3. Copy ***liblinear.dll*** to output directory; &lt;Pendigits_dir&gt;\bin\Release\netcoreapp2.0.

**NOTE**  
If you want to run at Linux, you should build the **LibLinearDotNet** at first.  
Please refer the [Tutorial for Linux](https://github.com/takuya-takeuchi/LibLinearDotNet/wiki/Tutorial-for-Linux).


## 3. Run

1. Open command prompt and change to &lt;Pendigits_dir&gt;
1. Type the following sample command
````
dotnet run -c Release -- "-o=trained.model" "-s=1"
````
After this, command prompt will show result like the following.
````
Accuracy: 79.7027% (2788/3498), Elapsed: 3300ms
````
If specify the regression parameter for "-s", the result will be like the following.
````
Mean squared error: 6.60231%, Squared correlation coefficient: 0.215259, Elapsed: 15ms
````

## 4. Et cetera

This program support the following argument and option.

### Argument

|Argument|Description|
|:-----------|:------------|
|quiet|Supress the LIBLINEAR output|

### Option

|Option|Short form|Description|
|:-----------|:------------|:------------|
|--output|-o|Output trained model to specified file path|
|--solver|-s|Specify the solver type.</br></br>***For multi-class classification***</br>0 : L2-regularized logistic regression (primal)</br>1 : L2-regularized L2-loss support vector classification (dual)</br>2 : L2-regularized L2-loss support vector classification (primal)</br>3 : L2-regularized L1-loss support vector classification (dual)</br>4 : support vector classification by Crammer and Singer</br>5 : L1-regularized L2-loss support vector classification</br>6 : L1-regularized logistic regression</br>7 : L2-regularized logistic regression (dual)</br></br>***For regression***</br>11 : L2-regularized L2-loss support vector regression (primal)</br>12 : L2-regularized L2-loss support vector regression (dual)</br>13 : L2-regularized L1-loss support vector regression (dual)|
|--bias|-b|Specify the bias|