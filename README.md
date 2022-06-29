# Automatic aim on target in camera frustrum + 3D arrow trajectory algorithms

__This project consist of:__
* __Input handling:__
  * Input handling is based on "Chop chop" Open Projects and new Unity Input System. 
  * Main difference is that "InputReader" class is no longer a ScriptbleObject, but just a MonoBehaviour.
  * [Link to video from Open Projects](https://www.youtube.com/watch?v=u1Zel20rwOk&list=PLX2vGYjWbI0S6CnkDm0AwVgA6E6L_vJNf&index=7).
* __Automatic target chooser:__
  * "TargetChooser" class is responsible for choosing object as a target.
  * First it detects all "ITargetable" objects in the scene that are in camera view frustrum.
  * Then it sets "ITargetable" object, that is closest to the shooter, as a target.
  * WARNING: currently "TargetChooser" does not detect if something is obstructing line of view to target.
    <br />
    It was deliberate decision to not implement this, because main purpose of project was to test trajectories of projectiles,
    <br />
    and it was easier to do that without worrying about obstruction of objects.
    <br />
    If you need, just implement this functionality by simple raycasting or something more advanced.
* __Weapon system and projectiles trajectory methods:__
  * There are two approach of computing trajectory of projectile:
    <br />
    Height based and velocity based.
    <br />
    In most cases you can choose one and then remove unnecessary code, thus simplifying code.
  * There are two weapon types (ScriptableObjects):
    * Height based
      * You set how projectile movement will look like based on height of the parable (arc height in inflection point).
    * Velocity based
      * You set how projectile movement will look like based on intial velocity of projectile when launched.
  * "WeaponHandler" class:
    * Responsible only for switching weapon type(height or velocity based) and invoking launching of projectile
  * Launching of projectiles is handled by one of two classes:
    * "HeightBasedProjectileLauncher"
      * This class will compute initial velocity and angle that should be used to launch projectile.
        <br />
        Projectile will travel along arc of given height (obtained from "WeaponType").
        <br />
        There is also an option to flatten the arc based on the distance between shooter and target.
        <br />
        The closer the shooter is to the target, the more flattened the trajectory becomes.
      * WARNING: if the shooter and the target are not on the same level ground (their 'y' positions are different) 
        <br /> the height of the arc will be modified to compensate for the difference.
        <br />
        Depending on your needs you can change that part of the code.
    * "VelocityBasedProjectileLauncher"
      * This class will compute initial angle that should be used to launch projectile.
        <br />
        Projectile will travel along arc computed based of initial velocity (obtained from "WeaponType").
      * Depending on the situation, the equation for computing angle of the launch requires diffren equation transformations.
        <br />
        There are three main situations:
        * Shooter and target are on the same level (their 'y' positions are the same)
        * Shooter is below target
        * Shooter is above target
      * All trajectory code is commented pretty heavily so it would be easier to understand equation transformations.
        <br />
        There are also links to videos that exmplains all equations used in code.
* __Arrow handling:__
  * "ArrowControler" class
    * Responsible only for destroying of the arrow
  * "ArrowLaunchData" class:
    * Data needed to launch arrow (eg. intial velocity, angle and etc) obtained from projectile launcher.
  * "ArrowMover" class:
    * Responsible for launching arrow (sets initial velocity on rigidbody)
    * Responsible also for pointing the tip of the arrow to match the trajectory
  * ArrowPositionPredicter" class:
    * Responsible for drawing projectile trajectory
* __Custom character controller:__
  * Physics based character controller + Cinemachine
* __Interactables:__
  * "PickUpHandler" class:
    * Used to switch weapon types
  * "Elevator" class:
    * Responsible for handling elevators
* __Arrow handling:__
      

<br />
<br />

# __Project preview:__
![Preview](https://github.com/mnijaki/AutoAimBowAndArrow/blob/b3e5406dbee9c84b709eb6a03740798a14b44818/Screenshots/main_gif.gif)

# __Screenshots:__
![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_1.png) 

![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_2.png)

![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_3.png)

![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_4.png)

![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_5.png)

![image](https://github.com/mnijaki/AutoAimBowAndArrow/blob/9a3b9246ec5eb74ee1420b79a87baf8a620bab1a/Screenshots/Screenshot_6.png)
