# Cross Validation

This exsample demonstrates cross validation process of LIBLINEAR.

## How to use?

## 1. Preparation

This sample requires train data set.  
Please download data set file from the following page.

* https://www.csie.ntu.edu.tw/~cjlin/libsvmtools/datasets/

At last, deploy extracted files to &lt;CrossValidation_dir&gt;.

## 2. Build

1. Open command prompt and change to &lt;CrossValidation_dir&gt;
1. Type the following command
````
dotnet build -c Release
````
2. Copy ***liblinear.dll*** to output directory; &lt;CrossValidation_dir&gt;\bin\Release\netcoreapp2.0.

**NOTE**  
If you want to run at Linux, you should build the **LibLinearDotNet** at first.  
Please refer the [Tutorial for Linux](https://github.com/takuya-takeuchi/LibLinearDotNet/wiki/Tutorial-for-Linux).


## 3. Run

1. Open command prompt and change to &lt;CrossValidation_dir&gt;
1. Type the following sample command
````
dotnet run -c Release -- "quiet" "-t=pendigits" "-s=7" "-b=3" "-f=4"
````
After this, command prompt will show result like the following.
````
Accuracy: 93.4614% (7004/7494), Elapsed: 49529ms
````
If specify the regression parameter for "-s", the result will be like the following.
````
Mean squared error: 5.27708%, Squared correlation coefficient: 0.362368, Elapsed: 40ms
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
|--solver|-s|Specify the solver type.</br></br>***For multi-class classification***</br>0 : L2-regularized logistic regression (primal)</br>1 : L2-regularized L2-loss support vector classification (dual)</br>2 : L2-regularized L2-loss support vector classification (primal)</br>3 : L2-regularized L1-loss support vector classification (dual)</br>4 : support vector classification by Crammer and Singer</br>5 : L1-regularized L2-loss support vector classification</br>6 : L1-regularized logistic regression</br>7 : L2-regularized logistic regression (dual)</br></br>***For regression***</br>11 : L2-regularized L2-loss support vector regression (primal)</br>12 : L2-regularized L2-loss support vector regression (dual)</br>13 : L2-regularized L1-loss support vector regression (dual)|
|--bias|-b|Specify the bias|
|--training|-t|Specify training set file|
|--fold|-f|Specify K-fold. (An integer of not less than 0)|