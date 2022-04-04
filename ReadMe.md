# Server Module [CSharp]

### 1. Logger Class
* Use Factory method design pattern
* Use Log4net Lib


### 2. Object Pool
* Thread safe
* can use various object type
* queue / lock, atomic (The best performance)

### 3. ThreadManager 
* Manage custom Threads (ex:accept, queueing etc...)<br>
(Lifestyle of Managing thread is very long, it will be terminated when program ends)

### 4. ThreadPool
* Set ThreadPool' min/max number of thread

### 5. Serialize/Deserialize (Packet)
* Performance Check
  * BitConverter.GetBytes
  * BitConverter.TryWriteBytes / ReadOnlySpan

### 6. DB
* DB Module
  * Single query (DML)
  * Stored procedure
  * Bulk insert
  * Transaction

### 7. Design Pattern
* Behavioral Pattern
  * State
* Creational Pattern
  * Sigleton
* Structural Pattern
  
