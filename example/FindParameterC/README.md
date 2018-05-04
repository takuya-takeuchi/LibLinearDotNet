# Find Parameter C
 
The problem is what find best C parameter from the problem contains float values 0 or greater but less than 10 into 10 classes.

## How to use?

## 1. Build

1. Open command prompt and change to &lt;FindParameterC_dir&gt;
1. Type the following command
````
dotnet build -c Release
````
2. Copy ***liblinear.dll*** to output directory; &lt;FindParameterC_dir&gt;\bin\Release\netcoreapp2.0.

**NOTE**  
If you want to run at Linux, you should build the **LibLinearDotNet** at first.  
Please refer the [Tutorial for Linux](https://github.com/takuya-takeuchi/LibLinearDotNet/wiki/Tutorial-for-Linux).


## 2. Run

1. Open command prompt and change to &lt;FindParameterC_dir&gt;
1. Type the following sample command
````
dotnet run -c Release -- "-s=0" "-b=3" "-sc=1" "-ec=10" "-f=5"
````
After this, command prompt will show result like the following.
````
BestC: 8, BestRate: 0.8588, Elapsed: 73938ms
````

## 3. Et cetera

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
|--fold|-f|Specify K-fold. (An integer of not less than 0)|
|--startc|-sc|Specify start of C parameter|
|--maxc|-mc|Specify max of C parameter|