using System;
using System.Runtime.InteropServices;

namespace LibLinearDotNet
{

    /// <summary>
    /// Represents an parameter for Large Linear Classification.
    /// </summary>
    public sealed class Parameter
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        public Parameter()
        {
        }

        internal unsafe Parameter(NativeMethods.parameter* param)
        {
            // NOTE:
            // In load_model function, param.init_sol is assigned as NULL.
            // It means that init_sol won't be saved by save_model function.

            this.Epsilon = param->eps;
            this.C = param->C;
            this.SolverType = (SolverType)param->solver_type;
            this.LengthOfWeight = param->nr_weight;

            if (this.LengthOfWeight > 0)
            {
                var darray = new double[this.LengthOfWeight];
                Marshal.Copy((IntPtr)param->weight, darray, 0, this.LengthOfWeight);
                this.Weight = darray;

                var iarray = new int[this.LengthOfWeight];
                Marshal.Copy((IntPtr)param->weight_label, iarray, 0, this.LengthOfWeight);
                this.WeightLabel = iarray;
            }
            else
            {
                this.Weight = null;
                this.WeightLabel = null;
            }

            this.P = param->p;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the cost of constraints violation parameter.
        /// </summary>
        public double C
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameter of stopping criterion.
        /// </summary>
        public double Epsilon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the initial solution.
        /// </summary>
        /// <remarks>If <see cref="Problem.Number"/> is not 2, the array length should be <see cref="Problem.Number"/> * <see cref="Model.Classes"/> + <see cref="Model.Classes"/>. Otherwise the array length should be <see cref="Problem.Number"/>.</remarks>
        public double[] InitialSolution
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of elements in the array <see cref="Weight"/> and <see cref="WeightLabel"/>.
        /// </summary>
        public int LengthOfWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sensitiveness of loss of support vector regression.
        /// </summary>
        public double P
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of solver.
        /// </summary>
        public SolverType SolverType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the array of factors to change penalty for some class.
        /// </summary>
        /// <remarks>Each <code>Weight[i]</code> corresponds to <code>WeightLabel[i]</code>.</remarks>
        public double[] Weight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the array of labels to change penalty for some class.
        /// </summary>
        /// <remarks>Each <code>Weight[i]</code> corresponds to <code>WeightLabel[i]</code>.</remarks>
        public int[] WeightLabel
        {
            get;
            set;
        }

        #endregion

    }

}