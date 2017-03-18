#pragma once

#include "IStepCommand.h"
#include "StepParameters.h"
#include "PathToFeature.h"
#include "SetupTearDownSlot.h"

#ifdef __COUNTER__
#define UNIQUE_NUM __COUNTER__
#else
#define UNIQUE_NUM __LINE__
#endif

#define CONCAT_(x,y) x##y
#define CONCAT(x,y) CONCAT_(x,y)
#define TO_WSTRING(text) CONCAT(L, text)
#define YourTestModelClass int
#define FEATURE_OF_STEP bdd::PathToFeature::ToFeature(TO_WSTRING(__FILE__)) 
#define UNIQUE_CLASS_NAME CONCAT(__StepImp, UNIQUE_NUM)
#define UNIQUE_VAR_NAME CONCAT(__var, UNIQUE_NUM)
#define UNIQUE_SETUP_CLASS_NAME CONCAT(__Setup, UNIQUE_NUM)
#define UNIQUE_TEARDOWN_CLASS_NAME CONCAT(__Teardown, UNIQUE_NUM)

//////////////////////////////// SETUP /////////////////////////////////
#define SETUP_IMP(class_name, test_model_class)\
namespace {\
test_model_class* model = NULL;\
struct class_name : public bdd::SetupSlot\
{\
    class_name() : SetupSlot(FEATURE_OF_STEP) {}\
    void DoSetup() override {\
        model = new test_model_class();\
        DoSetup_internal();\
    }\
    void DoSetup_internal();\
};\
class_name UNIQUE_VAR_NAME;\
}\
void class_name::DoSetup_internal()

#define SETUP(test_model_class) SETUP_IMP(UNIQUE_SETUP_CLASS_NAME, test_model_class)

//////////////////////////////// TEAR_DOWN /////////////////////////////////

#define TEAR_DOWN_IMP(class_name)\
namespace {\
struct class_name : public bdd::TearDownSlot\
{\
    class_name() : TearDownSlot(FEATURE_OF_STEP) {}\
    void DoTearDown() override {\
        DoTearDown_internal();\
        delete model;\
        model = NULL;\
    }\
    void DoTearDown_internal();\
};\
class_name UNIQUE_VAR_NAME;\
}\
void class_name::DoTearDown_internal()

#define TEAR_DOWN() TEAR_DOWN_IMP(UNIQUE_TEARDOWN_CLASS_NAME)

//////////////////////////////// STEP0 /////////////////////////////////
#define STEP_IMP0(class_name, step_pattern)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters&) override\
    { command(); }\
    void command();\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command()

#define STEP0(step_pattern)\
STEP_IMP0(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern))

//////////////////////////////// STEP1 /////////////////////////////////
#define STEP_IMP1(class_name, step_pattern, type1, arg1)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0])); }\
    void command(type1);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1)

#define STEP1(step_pattern, type1, arg1)\
STEP_IMP1(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1)

//////////////////////////////// STEP2 /////////////////////////////////
#define STEP_IMP2(class_name, step_pattern, type1, arg1, type2, arg2)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0]), GetParam<type2>(params[1])); }\
    void command(type1, type2);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1, type2 arg2)

#define STEP2(step_pattern, type1, arg1, type2, arg2)\
STEP_IMP2(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1, type2, arg2)

//////////////////////////////// STEP3 /////////////////////////////////
#define STEP_IMP3(class_name, step_pattern, type1, arg1, type2, arg2, type3, arg3)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0]), GetParam<type2>(params[1]), GetParam<type3>(params[2])); }\
    void command(type1, type2, type3);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1, type2 arg2, type3 arg3)

#define STEP3(step_pattern, type1, arg1, type2, arg2, type3, arg3)\
STEP_IMP3(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1, type2, arg2, type3, arg3)

//////////////////////////////// STEP4 /////////////////////////////////
#define STEP_IMP4(class_name, step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0]), GetParam<type2>(params[1]), GetParam<type3>(params[2]), GetParam<type4>(params[3])); }\
    void command(type1, type2, type3, type4);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1, type2 arg2, type3 arg3, type4 arg4)

#define STEP4(step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4)\
STEP_IMP4(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1, type2, arg2, type3, arg3, type4, arg4)

//////////////////////////////// STEP5 /////////////////////////////////
#define STEP_IMP5(class_name, step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0]), GetParam<type2>(params[1]), GetParam<type3>(params[2]), GetParam<type4>(params[3]), GetParam<type5>(params[4])); }\
    void command(type1, type2, type3, type4, type5);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1, type2 arg2, type3 arg3, type4 arg4, type5 arg5)

#define STEP5(step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5)\
STEP_IMP5(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5)

//////////////////////////////// STEP6 /////////////////////////////////
#define STEP_IMP6(class_name, step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5, type6, arg6)\
namespace {\
class class_name : public bdd::IStepCommand\
{\
public:\
    class_name(const std::wstring pattern) :\
        IStepCommand(FEATURE_OF_STEP, pattern){}\
    void Execute(StepParameters& params) override\
    { command(GetParam<type1>(params[0]), GetParam<type2>(params[1]), GetParam<type3>(params[2]), GetParam<type4>(params[3]), GetParam<type5>(params[4]), GetParam<type6>(params[5])); }\
    void command(type1, type2, type3, type4, type5, type6);\
};\
class_name UNIQUE_VAR_NAME(step_pattern);\
}\
void class_name::command(type1 arg1, type2 arg2, type3 arg3, type4 arg4, type5 arg5, type6 arg6)

#define STEP6(step_pattern, type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5, type6, arg6)\
STEP_IMP6(UNIQUE_CLASS_NAME, TO_WSTRING(step_pattern), type1, arg1, type2, arg2, type3, arg3, type4, arg4, type5, arg5, type6, arg6)
