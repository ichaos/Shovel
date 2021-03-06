// Copyright (c) 2012, Miron Brezuleanu
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace ShovelTests
{
    [TestFixture]
    public class VmTests
    {
        [Test]
        public void Context1 ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var id = fn x x
var h = fn (x) context
var g = fn (x) id(h(x))
var f = fn (x) id(g(x))
f(3)
"
            );
            var result = Shovel.Api.TestRunVm (sources);
            Assert.IsTrue (result.Kind == Shovel.Value.Kinds.Hash);
            Assert.AreEqual (2, result.HashValue.Keys.Count);
            Assert.IsTrue (result.HashValue.ContainsKey (Shovel.Value.Make ("stack")));
            Assert.IsTrue (result.HashValue.ContainsKey (Shovel.Value.Make ("environment")));
            Assert.AreEqual (@"Frame starts at:
file 'test.sho' line 3: var h = fn (x) context
file 'test.sho' line 3:             ^
Frame variables are:
x = 3

Frame starts at:
file 'test.sho' line 2: var id = fn x x [...content snipped...]
file 'test.sho' line 2: ^^^^^^^^^^^^^^^
Frame variables are:
id = [...callable...]
h = [...callable...]
g = [...callable...]
f = [...callable...]

", result.HashValue [Shovel.Value.Make ("environment")].StringValue);
            Assert.AreEqual (@"file 'test.sho' line 3: var h = fn (x) context
file 'test.sho' line 3:                ^^^^^^^
file 'test.sho' line 4: var g = fn (x) id(h(x))
file 'test.sho' line 4:                   ^^^^
file 'test.sho' line 5: var f = fn (x) id(g(x))
file 'test.sho' line 5:                   ^^^^
file 'test.sho' line 6: f(3)
file 'test.sho' line 6: ^^^^
", result.HashValue [Shovel.Value.Make ("stack")].StringValue);
        }

        [Test]
        public void Context2 ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var h = fn (x) context
var g = fn (x) h(x)
var f = fn (x) g(x)
f(3)
"
            );
            var result = Shovel.Api.TestRunVm (sources);
            Assert.IsTrue (result.Kind == Shovel.Value.Kinds.Hash);
            Assert.AreEqual (2, result.HashValue.Keys.Count);
            Assert.IsTrue (result.HashValue.ContainsKey (Shovel.Value.Make ("stack")));
            Assert.IsTrue (result.HashValue.ContainsKey (Shovel.Value.Make ("environment")));
            Assert.AreEqual (@"Frame starts at:
file 'test.sho' line 2: var h = fn (x) context
file 'test.sho' line 2:             ^
Frame variables are:
x = 3

Frame starts at:
file 'test.sho' line 2: var h = fn (x) context [...content snipped...]
file 'test.sho' line 2: ^^^^^^^^^^^^^^^^^^^^^^
Frame variables are:
h = [...callable...]
g = [...callable...]
f = [...callable...]

", result.HashValue [Shovel.Value.Make ("environment")].StringValue);
            Assert.AreEqual (@"file 'test.sho' line 2: var h = fn (x) context
file 'test.sho' line 2:                ^^^^^^^
file 'test.sho' line 5: f(3)
file 'test.sho' line 5: ^^^^
", result.HashValue [Shovel.Value.Make ("stack")].StringValue);
        }

        [Test]
        public void Factorial ()
        {
            Utils.TestValue (Utils.FactorialOfTenProgram (), Shovel.Value.Kinds.Integer, (long)3628800);
        }

        [Test]
        public void Fibonacci ()
        {
            Utils.TestValue (Utils.FibonacciOfTenProgram (), Shovel.Value.Kinds.Integer, (long)89);
        }

        [Test]
        public void QuickSort ()
        {
            var sources = Shovel.Api.MakeSourcesWithStdlib ("qsort.sho", Utils.QsortProgram ());
            var result = Shovel.Api.TestRunVm (sources);
            Assert.IsTrue (result.Kind == Shovel.Value.Kinds.Array);
            Assert.AreEqual (5, result.ArrayValue.Count);
            Assert.AreEqual (1, result.ArrayValue [0].IntegerValue);
            Assert.AreEqual (2, result.ArrayValue [1].IntegerValue);
            Assert.AreEqual (3, result.ArrayValue [2].IntegerValue);
            Assert.AreEqual (4, result.ArrayValue [3].IntegerValue);
            Assert.AreEqual (5, result.ArrayValue [4].IntegerValue);
        }

        [Test]
        public void NonLocalReturn ()
        {
            Utils.TestValue (@"
var h = fn x x + 2
var g = fn x h(x) + 2
var f = fn x block 'f' g(x) + 2
f(1)
", Shovel.Value.Kinds.Integer, (long)7);

            Utils.TestValue (@"
var h = fn x return 'f' 10
var g = fn x h(x) + 2
var f = fn x block 'f' g(x) + 2
f(1)
", Shovel.Value.Kinds.Integer, (long)10);
        }

        [Test]
        public void ManyArgs2 ()
        {
            Utils.TestValue (@"
var g = fn (a, b) a - b
var f = fn (a, b) g(a, b) + 1
f(1, 2)
", Shovel.Value.Kinds.Integer, (long)0);
        }

        [Test]
        public void ManyArgs3 ()
        {
            Utils.TestValue (@"
var g = fn (a, b, c) (a - b) * c
var f = fn (a, b, c) g(a, b, c) + 1
f(1, 2, 3)
", Shovel.Value.Kinds.Integer, (long)-2);
        }

        [Test]
        public void ManyArgs4 ()
        {
            Utils.TestValue (@"
var g = fn (a, b, c, d) (a - b) * (c - d)
var f = fn (a, b, c, d) g(a, b, c, d) + 1
f(1, 2, 3, 5)
", Shovel.Value.Kinds.Integer, (long)3);
        }

        [Test]
        public void UserDefinedPrimitives ()
        {
            List<string> log = new List<string> ();
            Action<Shovel.VmApi, Shovel.Value[], Shovel.UdpResult> print = (api, args, result) => {
                if (args.Length > 0 && args [0].Kind == Shovel.Value.Kinds.String) {
                    log.Add (args [0].StringValue);
                } else {
                    throw new InvalidOperationException ();
                }
            };
            var sources = Shovel.Api.MakeSourcesWithStdlib ("test.sho", @"
var a = 0
stdlib.repeat(3, fn () {
    a = a + 1
    @print(string(a))
})
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            Shovel.Api.RunVm (bytecode, sources, new Shovel.Callable[] {
                Shovel.Callable.MakeUdp ("print", print, 1),
            }
            );
            Assert.AreEqual (3, log.Count);
            Assert.AreEqual ("1", log [0]);
            Assert.AreEqual ("2", log [1]);
            Assert.AreEqual ("3", log [2]);
        }

        static IEnumerable<Shovel.Callable> GetPrintAndStopUdps (List<string> log, bool retryStop)
        {
            Action<Shovel.VmApi, Shovel.Value[], Shovel.UdpResult> print = (api, args, result) => {
                if (args.Length > 0 && args [0].Kind == Shovel.Value.Kinds.String) {
                    log.Add (args [0].StringValue);
                } else {
                    throw new InvalidOperationException ();
                }
            };
            Action<Shovel.VmApi, Shovel.Value[], Shovel.UdpResult> stop;
            if (retryStop) {
                bool firstCallOfStop = true;
                stop = (api, args, result) => {
                    if (firstCallOfStop) {
                        result.After = Shovel.UdpResult.AfterCall.NapAndRetryOnWakeUp;
                        firstCallOfStop = false;
                    } else {
                        result.After = Shovel.UdpResult.AfterCall.Continue;
                        result.Result = Shovel.Value.Make ("world");
                    }
                };

            } else {
                stop = (api, args, result) => {
                    result.After = Shovel.UdpResult.AfterCall.Nap;
                };
            }
            return new Shovel.Callable[] {
                Shovel.Callable.MakeUdp ("print", print, 1),
                Shovel.Callable.MakeUdp ("stop", stop, 0),
            };
        }

        [Test]
        public void StopAndWakeUp ()
        {
            List<string> log = new List<string> ();
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var a = ""hello, ""
var b = ""world""
@stop()
@print(string(a + b))
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            Shovel.Api.WakeUpVm (vm);
            Shovel.Api.RunVm (vm, sources, userPrimitives);
            Assert.AreEqual (1, log.Count);
            Assert.AreEqual ("hello, world", log [0]);
        }

        [Test]
        public void StopWakeUpAndRetry ()
        {
            List<string> log = new List<string> ();
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var a = ""hello, ""
var b = @stop()
@print(string(a + b))
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, true);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            Shovel.Api.WakeUpVm (vm);
            Shovel.Api.RunVm (vm, sources, userPrimitives);
            Assert.AreEqual (1, log.Count);
            Assert.AreEqual ("hello, world", log [0]);
        }

        [Test]
        public void StopSerializeWakeUpAndRetry ()
        {
            List<string> log = new List<string> ();
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var a = ""hello, ""
var b = @stop()
@print(string(a + b))
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, true);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (1, log.Count);
            Assert.AreEqual ("hello, world", log [0]);
        }

        [Test]
        public void StopSerializeWakeUp ()
        {
            List<string> log = new List<string> ();
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var a = ""hello, ""
var b = ""world""
@stop()
@print(string(a + b))
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (1, log.Count);
            Assert.AreEqual ("hello, world", log [0]);
        }

        [Test]
        public void CachedFramesAndSimpleObjects ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
var c2 = makeCounter()
c1()
c1()
c2()
c2()
c2()
@print(string(c1()))
@print(string(c2()))
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            Assert.AreEqual (2, log.Count);
            Assert.AreEqual ("3", log [0]);
            Assert.AreEqual ("4", log [1]);
        }

        [Test]
        public void SerializeSimpleObjects ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
var c2 = makeCounter()
c1()
c1()
c2()
c2()
c2()
@stop()
@print(string(c1()))
@print(string(c2()))
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (2, log.Count);
            Assert.AreEqual ("3", log [0]);
            Assert.AreEqual ("4", log [1]);
        }

        [Test]
        public void SerializeWithReturnAddressOnStack ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
var c2 = makeCounter()
var main = fn () {
  c1()
  c1()
  c2()
  c2()
  c2()
  @stop()
  @print(string(c1()))
  @print(string(c2()))
}
main()
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (2, log.Count);
            Assert.AreEqual ("3", log [0]);
            Assert.AreEqual ("4", log [1]);
        }

        [Test]
        public void SerializeWithArray ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var main = fn () {
  var arr = array(1, 2, 3, 4)
  @print(string(arr[0]))
  @print(string(arr[1]))
  @stop()
  @print(string(arr[2]))
  @print(string(arr[3]))
}
main()
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (4, log.Count);
            Assert.AreEqual ("1", log [0]);
            Assert.AreEqual ("2", log [1]);
            Assert.AreEqual ("3", log [2]);
            Assert.AreEqual ("4", log [3]);
        }

        [Test]
        public void SerializeWithHash ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var main = fn () {
  var h = hash('a', 1, 'b', 2)
  @print(string(h.a))
  @stop()
  @print(string(h.b))
}
main()
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            Assert.AreEqual (2, log.Count);
            Assert.AreEqual ("1", log [0]);
            Assert.AreEqual ("2", log [1]);
        }

        [Test]
        public void NonLocalReturnAndCachedFrames ()
        {
            Utils.TestValue (@"
var g = fn (x) if x == 3 return 'b' 10 else x
var f = fn (x) g(x) + 3
block 'b' f(2) + f(3)
", Shovel.Value.Kinds.Integer, (long)10);
        }

        [Test]
        public void RestoreStateWithWrongBytecode ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var main = fn () {
  var h = hash('a', 1, 'b', 2)
  @print(string(h.a))
  @stop()
  @print(string(h.b))
}
main()
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            var state = Shovel.Api.SerializeVmState (vm);
            sources = Shovel.Api.MakeSources ("test.sho", @"'hello'");
            bytecode = Shovel.Api.GetBytecode (sources);
            Utils.ExpectException<Shovel.Exceptions.BytecodeDoesntMatchState> (() => {
                Shovel.Api.RunVm (bytecode, sources, userPrimitives, state);
            }, 
            ex => {
                Assert.IsNotNull (ex);
            }
            );
        }

        [Test]
        public void CellsQuota ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
block 'c' {
  block 'b' {
    c1()
  }
}
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            Utils.ExpectException<Shovel.Exceptions.ShovelCellQuotaExceededException> (() => {
                Shovel.Api.RunVm (bytecode, sources, usedCellsQuota: 30);
            },
            (ex) => {
                Assert.IsNotNull (ex);
            }
            );
        }

        [Test]
        public void ExecutedTicksQuota ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
block 'c' {
  block 'b' {
    c1()
  }
}
c1()
c1()
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            Utils.ExpectException<Shovel.Exceptions.ShovelTicksQuotaExceededException> (() => {
                Shovel.Api.RunVm (bytecode, sources, totalTicksQuota: 20);
            },
            (ex) => {
                Assert.IsNotNull (ex);
            }
            );
        }

        [Test]
        public void TicksUntilNextNapQuota ()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
var makeCounter = fn () {
  var counter = 0
  fn () counter = counter + 1
}
var c1 = makeCounter()
block 'c' {
  block 'b' {
    c1()
  }
}
c1()
c1()
"
            );
            var bytecode = Shovel.Api.GetBytecode (sources);
            var vm = Shovel.Api.RunVm (bytecode, sources, ticksUntilNextNapQuota: 20);
            Assert.IsFalse (Shovel.Api.VmIsLive (vm));
            Assert.IsFalse (Shovel.Api.VmExecutionComplete (vm));
            Shovel.Api.WakeUpVm (vm);
            Shovel.Api.RunVm (vm, sources, ticksUntilNextNapQuota: 10);
            Assert.IsFalse (Shovel.Api.VmIsLive (vm));
            Assert.IsFalse (Shovel.Api.VmExecutionComplete (vm));
            Shovel.Api.WakeUpVm (vm);
            Shovel.Api.RunVm (vm, sources, ticksUntilNextNapQuota: 20);
            Assert.IsFalse (Shovel.Api.VmIsLive (vm));
            Assert.IsTrue (Shovel.Api.VmExecutionComplete (vm));
        }

        [Test]
        public void UdpError()
        {
            var sources = Shovel.Api.MakeSources ("test.sho", @"
@print(100)
"
            );
            List<string> log = new List<string> ();
            var bytecode = Shovel.Api.GetBytecode (sources);
            var userPrimitives = GetPrintAndStopUdps (log, false);
            var vm = Shovel.Api.RunVm (bytecode, sources, userPrimitives);
            Assert.IsNotNull(Shovel.Api.VmUserDefinedPrimitiveError(vm));
        }

        [Test]
        public void MultipleGrefs()
        {
            Utils.TestValue("var a = array(array(array(3))) a[0][0][0]", Shovel.Value.Kinds.Integer, (long)3);
        }
    }
}
