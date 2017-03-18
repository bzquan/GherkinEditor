#ifndef GMockMacro_h
#define GMockMacro_h

// Veryfy 1 mock object and report failure
#define CHECK_GMOCK_EXPECTATION1(mock1) ::testing::Mock::VerifyAndClearExpectations(mock1);
 
// Veryfy 2 mock objects and report failure
#define CHECK_GMOCK_EXPECTATION2(mock1, mock2) ::testing::Mock::VerifyAndClearExpectations(mock1);\
	::testing::Mock::VerifyAndClearExpectations(mock2);

// Veryfy 3 mock objects and report failure
#define CHECK_GMOCK_EXPECTATION3(mock1, mock2, mock3) ::testing::Mock::VerifyAndClearExpectations(mock1);\
	::testing::Mock::VerifyAndClearExpectations(mock2);\
	::testing::Mock::VerifyAndClearExpectations(mock3);

// Veryfy 4 mock objects and report failure
#define CHECK_GMOCK_EXPECTATION4(mock1, mock2, mock3, mock4) ::testing::Mock::VerifyAndClearExpectations(mock1);\
	::testing::Mock::VerifyAndClearExpectations(mock2);\
	::testing::Mock::VerifyAndClearExpectations(mock3);\
	::testing::Mock::VerifyAndClearExpectations(mock4);

// Veryfy 5 mock objects and report failure
#define CHECK_GMOCK_EXPECTATION5(mock1, mock2, mock3, mock4, mock5) ::testing::Mock::VerifyAndClearExpectations(mock1);\
	::testing::Mock::VerifyAndClearExpectations(mock2);\
	::testing::Mock::VerifyAndClearExpectations(mock3);\
	::testing::Mock::VerifyAndClearExpectations(mock4);\
	::testing::Mock::VerifyAndClearExpectations(mock5);

#endif