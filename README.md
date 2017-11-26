# XamarinBleSimpleController
使用Xamarin开发的Ble设备服务与特征值简单控制的Android程序 , 可以实现Ble设备的搜索 , 连接 , 服务浏览 , 特征值浏览(16进制与字符串两种预览方式) , 修改与注册监听的功能 ; 适合刚开始接触Xamarin开发Android程序的人和做Ble设备控制的人来参考 ;



**1. 蓝牙设备的搜索**

​	这里蓝牙设备可以从三个地方获取到 , 分别对应BlueToothAdapter的三个方法

		1. btAdapter.BondedDevices ;
		2. btAdapter.StartDiscovery() ;
		3. btAdapter.StartLeScan( new BLEScanCallBack()) ;

​	第一个是获取绑定的蓝牙设备 , 第二个是获取标准蓝牙设备(基于SPP协议的) , 第三个是获取低功耗蓝牙设备(也就是BLE , 基于GATT协议的) ;

​	但是StartLeScan()这个方法在android L被弃用了 , 为了兼容以前的版本还是可以使用 , 但是要在Xamarin中使用的话需要用try catch 捕获异常 , 比较麻烦 , 而且经过测试 , StartDiscovery()已经可以搜索到BLE设备了 , 所以没必要再多此一举的使用StartLeScan()这个方法了 ;

​	在StartDiscovery()后 , 搜索结果会通过广播发送回来 , 需要注册BREDRreceiver这个广播 , 对应的Action为BluetoothDevice.ActionFound , 需要实现 BREDRReceiver.IDeviceFound这个接口 ;

**2. 蓝牙设备的连接**

​	这里只针对BLE设备的连接进行说明 :

​	BLE和标准蓝牙的区别最大的一点就是不需要进行配对 , 就可以直接进行蓝牙连接(有点像wifi连接一样了) , 所以当搜索到蓝牙设备后 , 可以直接调用BluetoothDevice.ConnectGatt()来进行连接 , 注意 , 这个方法会返回一个BluetoothGatt的对象 , 你可以把它理解为手机与蓝牙设备之间的通道 , 是唯一的 , 接下来的操作都要基于这个对象来执行 , 如果需要界面跳转等操作的话 , 就要想办法把这个对象本身传递过去 , 最好不要用序列化等方式 , 这样可能会造成对象不一致的问题 ;

​	在调用了ConnectGatt()后 , 连接结果会通过回调中的OnConnectionStateChange()返回回来 , 在这里要注意了 , 有一个臭名昭著的问题 **status 133** , 实际上是*GATT_ERROR 0x85* , 也就是建立通信道路失败 , 根据网上其他的博客所说 , 是因为gatt的连接数超过了最大连接数造成的 , 所以连接之前或者断开连接后最好调用一下gatt.close()来释放一下资源 ;

**3. 蓝牙设备的服务读取**

​	蓝牙设备中有服务和服务的特征值 , 不管是服务还是特征值都有唯一的UUID与之对应 , 你可以这样理解 : 蓝牙设备 -> 项目 -> APP不能修改 , 服务 -> 实体类 -> APP不能修改 , 特征值 -> 实体类中的具体属性 -> APP可能可以修改(根据实际上的权限来判断的) ;

​	扫描BLE设备中的服务 , 只要在连接成功后使用之前拿到的GATT对象调用一下gatt.DiscoverServices()这个方法就可以了 , 没当扫描到了一个新的服务 , 扫描的结果会在注册的回调里返回 ;

​	这里我没有用这种方法 , 而是使用的gatt.services直接拿到的里面已经扫描到的服务 , 这种方法好处是不需要再回调里面增加一个监听 , 不好的地方是有可能会因为扫描的延时导致第一次获取到的服务不全 , 需要多拿一两次才能够拿全 ;

​	拿到服务了 , 就可以开始重头戏获取特征值了 ;

**4. 蓝牙设备特征值的读取 , 设置和监听**

​	获取到某一个关注的服务后 , 就可以获取特征值了 , 注意 , 在这里虽然可以通过服务拿到里面特征值的数量和特征值对象 , 但是以为这时候就可以直接通过这里的特征值对象拿到里面的value就太天真了 , 经过测试直接拿特征值的value : byte[]的话会返回空 , 后来发现需要使用gatt.ReadCharacteristic(Characteristic c)来读一下 , 然后通过最开始获取gatt对象那个ConnectGatt中传进去的Callback拿到对应的有值的Characteristic对象 , 接下来才能对它进行一系列操作 ;

​	读取就不多说了 , 通过回调拿到特征值对象以后直接.Values就能拿到一个byte[] , 这里唯一要注意的是 , 有些byte[] 是作为字符串显示的 , 另外一些是用来设置某些属性的 , 所以在解析的时候要注意解析方式是作为字符串还是作为别的什么 ;

​	设置的话就是把一个byte[]设置给特征值(characteristic.SetValue(newValue);) , 然后再调用gatt的写特征之的方法(gatt.WriteCharacteristic(characteristic);) , 就可以了 , 在写入成功后最初传入的callback还会被调用一个写入的方法 , 可以通过这个回调判断是否写入成功了 ;

​	设置监听(通知)的话需要分成以下几步进行 :

		1. 先把用gatt把特征值本身的属性修改一下 : gatt.SetCharacteristicNotification(characteristic, enable);
		2. 然后拿到Descriptor : BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
		3. 拿到BluetoothGattDescriptor.EnableNotificationValue对应的byte[] , 这里我用的是先自己创建一个同样长度的字节数组 , 然后分别把每一位的值传入进去 ;
		4. 调用descriptor.SetValue(value) , 把这个值传入到描述符里头 ;
		5. 接下来就用gatt.WriteDescriptor来写入这个描述符 , 这个写入方法会返回一个boolean , 来判断是否写入成功 ;
		6. 接下来就可以通过最初传入的callback中的OnCharacteristicChanged()来监听蓝牙设备中我们注册的特征值改变的通知了 ;
		7. 这里还有一点要注意 , 不是所有的属性都是可通知的 , 蓝牙设备端的每个特征值都有三个可选属性{get; set; notiry;} , 要看硬件那边的人是怎么设置这些属性的 , 如果一个特征值只有{get; set;} 那么就是注册了通知也不会被通知的 ;

**5. 一些设计思路**

	1. 我在回调中使用了List<interface>的方式来动态的添加和移除监听器 , 这样在View中注册监听器的时候就灵活了很多 , 显示的时候把监听器设置进去 , 不可见的时候就移除掉监听器了 , 但是如果这样的话在进行回调的时候很可能会出问题 , 也就是我正在遍历list然后给每一个监听器进行回调的时候 , 有一个界面的可见状态变化的话 , 就会崩溃 , 原因是在遍历的时候修改了这个list , 所以这里我增加了一个锁来保护这个list在使用的时候不会被改变 ;
	2. Xamarin的列表控件适配器与Java的比起来有一些区别 , 主要就是在holder的settag这里 , 在java中只要给convertview设置了tag为holder就可以了 , 但是Xamarin中就需要把holder中的每一个参数设置一下 , 而且还要设置int Key(一般可以理解为Resouce.Id , 也就是holder中参数对应的id) , 在getTag的时候要通过对应的Resouce.Id才能拿得到 , 个人感觉比Java中麻烦了好多 ;
	3. Xamarin由于是用C#来进行编程的 , 所以用了很多拉姆达表达式 , 这里一定要用熟练了 ;

​	

​	