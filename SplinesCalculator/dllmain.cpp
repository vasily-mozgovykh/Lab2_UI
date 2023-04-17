#include "pch.h"
#include "mkl_df.h"

#include <iostream>

enum spl_error {
	SPL_SUCCESS = 0,
	SPL_NEW_TASK = 1,
	SPL_MEM_ALLOC = 2,
	SPL_EDIT_SPLINE = 3,
	SPL_CONSTRUCT = 4,
	SPL_INTERPOLATE = 5,
	SPL_INTEGRATE = 6,
	SPL_DELETE_TASK = 7
};

extern "C" __declspec(dllexport) void calculate_spline(
	const double* nodes,
	const MKL_INT nodes_count,
	const bool is_uniform,
	const double* values,
	const double* boundary_conditions,
	const MKL_INT nsite,
	const double* site,
	double left_integration_limit,
	double right_integration_limit,
	double* spline_values,
	double* integral_value,
	int* error
) {
	/* By default there is no error */
	*error = SPL_SUCCESS;

	/* Create a task */
	DFTaskPtr task = nullptr;
	MKL_INT dimensions = 1;
	if (dfdNewTask1D(
		&task,
		nodes_count,
		nodes,
		is_uniform ? DF_UNIFORM_PARTITION : DF_NON_UNIFORM_PARTITION,
		dimensions,
		values,
		DF_NO_HINT
	) != DF_STATUS_OK) {
		*error = SPL_NEW_TASK;
		return;
	}

	/* Allocate memory for spline coefficients */
	double* scoeff = nullptr;
	if ((scoeff = new double[DF_PP_CUBIC * (nodes_count - 1)]) == nullptr) {
		*error = SPL_MEM_ALLOC;
		return;
	}

	/* Configure spline parameters */
	if (dfdEditPPSpline1D(
		task,
		DF_PP_CUBIC,
		DF_PP_NATURAL,
		DF_BC_1ST_LEFT_DER | DF_BC_1ST_RIGHT_DER,
		boundary_conditions,
		DF_NO_IC,
		nullptr,
		scoeff,
		DF_NO_HINT
	) != DF_STATUS_OK) {
		*error = SPL_EDIT_SPLINE;
		delete[] scoeff;
		return;
	}

	/* Construct a natural cubic spline */
	if (dfdConstruct1D(
		task,
		DF_PP_SPLINE,
		DF_METHOD_STD
	) != DF_STATUS_OK) {
		*error = SPL_CONSTRUCT;
		delete[] scoeff;
		return;
	}

	/* Calculate spline with its first and second derivatives */
	const MKL_INT ndorder = 3;
	const MKL_INT dorder[] = { 1, 1, 1 };
	if (dfdInterpolate1D(
		task,
		DF_INTERP,
		DF_METHOD_PP,
		nsite,
		site,
		DF_UNIFORM_PARTITION,
		ndorder,
		dorder,
		nullptr,
		spline_values,
		DF_MATRIX_STORAGE_ROWS,
		nullptr
	) != DF_STATUS_OK) {
		*error = SPL_INTERPOLATE;
		delete[] scoeff;
		return;
	}

	/* Integrate spline */
	const MKL_INT nlim = 1;
	if (dfdIntegrate1D(
		task,
		DF_METHOD_PP,
		nlim,
		&left_integration_limit,
		DF_NO_HINT,
		&right_integration_limit,
		DF_NO_HINT,
		nullptr,
		nullptr,
		integral_value,
		DF_NO_HINT
	) != DF_STATUS_OK) {
		*error = SPL_INTEGRATE;
		delete[] scoeff;
		return;
	}

	/* Free allocated memory and delete task */
	delete[] scoeff;
	if (dfDeleteTask(&task) != DF_STATUS_OK) {
		*error = SPL_DELETE_TASK;
	}
}