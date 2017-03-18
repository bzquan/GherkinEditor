#pragma once

#include <string>

namespace bdd
{
	class UndefinedStepException : public std::domain_error
	{
	public:
		explicit UndefinedStepException(const string& what_arg) : std::domain_error(what_arg)
		{
		}
	};
}
