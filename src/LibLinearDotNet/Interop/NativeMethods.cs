using System;
using System.Runtime.InteropServices;
using System.Security;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace LibLinearDotNet
{

    internal static unsafe class NativeMethods
    {

#if LINUX
        public const string CLibrary = "glibc.so";

        public const string NativeLibrary = "liblinear.so";

        public const CallingConvention CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
#else
        public const string CLibrary = "msvcrt.dll";

        public const string NativeLibrary = "liblinear.dll";

        public const CallingConvention CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
#endif

        #region Constants

        public const int OK = 0;

        public const int Error = -1;

        public const int True = 1;

        public const int False = 0;

        #endregion

        #region Methods

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern IntPtr check_parameter(problem* prob, parameter* param);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern int check_probability_model(model* model);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern int check_regression_model(model* model);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern void cross_validation(problem* prob, parameter* param, int nr_fold, double[] target);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern void find_parameter_C(problem *prob, parameter *param, int nr_fold, double start_C, double max_C, out double best_C, out double best_rate);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern double get_decfun_coef(IntPtr model, int feat_idx, int label_idx);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern double get_decfun_bias(IntPtr model, int label_idx);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern model* load_model([MarshalAs(UnmanagedType.LPStr)] string model_file_name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern double predict(IntPtr model, feature_node* x);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern double predict_probability(IntPtr model, feature_node* x, double[] prob_estimates);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern double predict_values(IntPtr model, feature_node* x, double[] dec_values);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern int save_model([MarshalAs(UnmanagedType.LPStr)]string model_file_name, model* model);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern model* train(problem* prob, parameter* param);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern void set_print_string_function(IntPtr func);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(CLibrary, EntryPoint = "memcpy", CallingConvention = CallingConvention)]
        public static extern IntPtr memcpy(int* dest, int* src, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(CLibrary, EntryPoint = "memcpy", CallingConvention = CallingConvention)]
        public static extern IntPtr memcpy(double* dest, double* src, int count);

        public static void free(IntPtr dest)
        {
            if (dest != IntPtr.Zero)
                Marshal.FreeCoTaskMem(dest);
        }

        public static IntPtr malloc(int size, int length)
        {
            return Marshal.AllocCoTaskMem(size * length);
        }

        #region model

        [StructLayout(LayoutKind.Explicit, Size = 96)]
        internal struct model
        {

            #region Fields

            [FieldOffset(0)]
            public parameter param;

            [FieldOffset(64)]
            public int nr_class;        /* number of classes */

            [FieldOffset(68)]
            public int nr_featurel;

            [FieldOffset(72)]
            public double* w;

            [FieldOffset(80)]
            public int* label;		   /* label of each class */

            [FieldOffset(88)]
            public double bias;

            #endregion

        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern void free_model_content(IntPtr model);

        #endregion

        #region feature_node

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        internal struct feature_node
        {

            #region Fields

            public int index;

            public double value;

            #endregion

        }

        #endregion

        #region parameter

        [StructLayout(LayoutKind.Explicit, Size = 64)]
        internal struct parameter
        {

            #region Fields

            [FieldOffset(0)]
            public int solver_type;

            /* these are for training only */
            [FieldOffset(8)]
            public double eps;      /* stopping criteria */

            [FieldOffset(16)]
            public double C;

            [FieldOffset(24)]
            public int nr_weight;

            [FieldOffset(32)]
            public int* weight_label;

            [FieldOffset(40)]
            public double* weight;

            [FieldOffset(48)]
            public double p;

            [FieldOffset(56)]
            public double* init_sol;

            #endregion

        }

        #endregion

        #region problem

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal struct problem
        {

            #region Fields

            [FieldOffset(0)]
            public int l;

            [FieldOffset(4)]
            public int n;

            [FieldOffset(8)]
            public double* y;

            [FieldOffset(16)]
            public feature_node** x;

            [FieldOffset(24)]
            public double bias;            /* < 0 if no bias term */

            #endregion

        }

        #endregion

        #endregion

    }

}
