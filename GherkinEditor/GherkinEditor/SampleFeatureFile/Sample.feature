Feature: Refund item
  Sales assistants should be able to refund customers' purchases.
  This is required by the law, and is also essential in order to
  keep customers happy.

  Rules:
  - Customer must present proof of purchase
  - Purchase must be less than 30 days ago

Background: 
  Given a $100 microwave was sold on 2015-11-03
  And today is 2015-11-18

@guid-094b3eaf-496d-4bca-aeb9-9218718f7f77
Scenario: feeding a small suckler cow
  Given the cow weighs 450 kg
  When we calculate the feeding requirements
  Then the energy should be 26500 MJ
  And the protein should be 215 kg

@guid-d731aa30-21f1-43ab-a106-7f60deceee0c
Scenario: feeding a medium suckler cow
  Given the cow weighs 500 kg
  When we calculate the feeding requirements
  Then the energy should be 29500 MJ
  And the protein should be 245 kg

# There are 2 more examples - I'm already bored
@guid-f4b7d22c-f16b-4457-9d5e-941710f8e17a
Scenario Outline: feeding a suckler cow
  Given the cow weighs <weight> kg
  When we calculate the feeding requirements
  Then the energy should be <energy> MJ
  And the protein should be <protein> kg

  Examples: 
    |weight|energy|protein|
    |450   |26500 |215    |
    |500   |29500 |245    |
    |575   |31500 |255    |
    |600   |37000 |305    |
