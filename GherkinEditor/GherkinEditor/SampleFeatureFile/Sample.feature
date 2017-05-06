Feature: Refund item
  Sales assistants should be able to refund customers' purchases.
  This is required by the law, and is also essential in order to
  keep customers happy.

  Rules:
  - Customer must present proof of purchase
  - Purchase must be less than 30 days ago

  Pictures are more descriptive.
  Here is a big Gherkin.
  ![Image 40](images/Gherkin.png)
  Here is a small Gherkin.
  ![Image 20](images/Gherkin.png)
  
  Sometimes mathematical formula is more easier to understand, e.g.
  ![LaTex](L = \sqrt{\frac {x^3 + 1}{2x^2 + 5x + 2}})

Background: 
  Given a $100 microwave was sold on 2015-11-03
  And today is 2015-11-18

Scenario: feeding a small suckler cow
  Given the cow weighs 450 kg
  When we calculate the feeding requirements
  Then the energy should be 26500 MJ
  And the protein should be 215 kg

Scenario: feeding a medium suckler cow
  Given the cow weighs 500 kg
  When we calculate the feeding requirements
  Then the energy should be 29500 MJ
  And the protein should be 245 kg

# There are 2 more examples - I'm already bored
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
