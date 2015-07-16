Simple-Object-Pooling
=====================
Copyright (c) 2014, Christopher Randin (cdrandin@gmail.com)
All rights reserved.

This is the first version of the Pooling System.
Main purpose was to get away from Instantiate a bunch of objects, 
but rather take away doing the same thing and focusing in keeping
track of the objects. Storing them for future use to recycle the 
objects.


To start this up ...

You could include my PoolingSystem prefab and have it all working 
out properly for you. The one caveat is to already have a list of 
objects you want to create and just keep track which one you want 
to create. Currently, the test is doing just that, but hopefully 
it should work without needing that. The only reason I do that is 
to keep track of the objects as well so I don't lose the reference 
to the object, thus not being able to return it back to the pool.
 I will later figure better ways around this to not have to worry, 
 but you could also use GameObject.Find(...), but I am not sure how 
 easy that would be to find the object you want.


There is only a single instance of the object which can be called 
with the following:

	`PoolingSystem.instance`
	
Create your objects the same way you would with Unity's Instantiate method;
Call:
	`PoolingSystem.instance.PS_Instantiate([GameObject]);`
	
Destroy your objects the same way you would with Unity's Destroy method;
Call:
	`PoolingSystem.instance.PS_Destroy([GameObject]);`
	
	
Any feedback would be awesome or any concerns about the software just let me know for anyway I can improve it.
