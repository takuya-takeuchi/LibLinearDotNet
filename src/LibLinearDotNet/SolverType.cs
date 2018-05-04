// ReSharper disable InconsistentNaming
namespace LibLinearDotNet
{

    /// <summary>
    /// The SolverType enumeration specifies the solver type for Large Linear Classification.
    /// </summary>
    public enum SolverType
    {

        /// <summary>
        /// L2-regularized logistic regression (primal).
        /// </summary>
        L2RegularizedLogisticRegression,

        /// <summary>
        /// L2-regularized L2-loss support vector classification (dual).
        /// </summary>
        L2RegularizedL2LossSupportVectorClassificationDual,

        /// <summary>
        /// L2-regularized L2-loss support vector classification (primal).
        /// </summary>
        L2RegularizedL2LossSupportVectorClassification,

        /// <summary>
        /// L2-regularized L1-loss support vector classification (dual).
        /// </summary>
        L2RegularizedL1LossSupportVectorClassificationDual,

        /// <summary>
        /// Support vector classification by Crammer and Singer.
        /// </summary>
        MulticlassSupportVectorMachineCrammerSinger,

        /// <summary>
        /// L1-regularized L2-loss support vector classification.
        /// </summary>
        L1RegularizedL2LossSupportVectorClassification,

        /// <summary>
        /// L1-regularized logistic regression.
        /// </summary>
        L1RegularizedLogisticRegression,

        /// <summary>
        /// L2-regularized logistic regression (dual).
        /// </summary>
        L2RegularizedLogisticRegressionDual,

        /// <summary>
        /// L2-regularized L2-loss support vector regression (primal).
        /// </summary>
        L2RegularizedL2LossSupportVectorRegression = 11,

        /// <summary>
        /// L2-regularized L2-loss support vector regression (dual).
        /// </summary>
        L2RegularizedL2LossSupportVectorRegressionDual,

        /// <summary>
        /// L2-regularized L1-loss support vector regression (dual).
        /// </summary>
        L2RegularizedL1LossSupportVectorRegressionDual
        
    }

}