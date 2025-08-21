# Uživatelská dokumentace

This is a library and a NuGet package that implements an (a,b)-tree with generic key of user's choice - as long as it derives from the IEnumerable interface.
(a,b)-tree is a data structure made up of nodes. 
Each node (except for the root) has to have at least _a_ children and maximum of _b_ children. Root has to have at least two children and maximum of _b_  children.

Methods to use:

Insert(key) - inserts key into tree and balances it

Delete(key) - deletes key from tree and balances it

Find(key) - find key in tree, or returns null if its not there

FindMin(key) - finds key with minimal value

FindMax(key) - finds key with maximal value

PrintNodes() - printing of tree


In ExampleProgram.cs there is shown how to use those methods, alongside comparison of time complexity with respect to a list already implemented.
