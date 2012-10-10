
(in-package #:shovel-tests)

(def-suite :shovel-tests)

(in-suite :shovel-tests)

(test test-constants
  (is (= 1 (shovel:naked-run-code (list "1"))))
  (is (string= "a" (shovel:naked-run-code (list "'a'"))))
  (is (= 1.23 (shovel:naked-run-code (list "1.23")))))

(test some-operators
  (is (= 2 (shovel:naked-run-code (list "1 + 1")))))

(test complex
  (is (equalp #(1 2 3 3 4 5)
              (shovel:naked-run-code
               (list (shovel:stdlib)
                     "stdlib.sort(array(3, 1, 2, 5, 4, 3), fn (a, b) a < b)")))))

(test tokenizer-error-message
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var g = fn x x + 2
var f = fn x g(x) + 2
f('1")))))
               "Shovel error in file 'test.shr' at end of file: Expected an end quote, but reached the end of file.

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var g = fn #x x + 2
var f = fn x g(x) + 2
f('1')")))))
               "Shovel error in file 'test.shr' at line 2, column 12: Unexpected character '#'.
file 'test.shr' line 2: var g = fn #x x + 2
file 'test.shr' line 2:            ^

")))

(test parser-error-message
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
b(]
")))))
               "Shovel error in file 'test.shr' at line 2, column 3: Unexpected token ']'.
file 'test.shr' line 2: b(]
file 'test.shr' line 2:   ^

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var a = fn [x] 1
")))))
               "Shovel error in file 'test.shr' at line 2, column 12: Expected a identifier, but got '['.
file 'test.shr' line 2: var a = fn [x] 1
file 'test.shr' line 2:            ^

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var fn = 1
")))))
               "Shovel error in file 'test.shr' at line 2, column 5: 'fn' is a reserved keyword.
file 'test.shr' line 2: var fn = 1
file 'test.shr' line 2:     ^^

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var slice = 1
")))))
               "Shovel error in file 'test.shr' at line 2, column 5: Name 'slice' is reserved for a primitive.
file 'test.shr' line 2: var slice = 1
file 'test.shr' line 2:     ^^^^^

")))

(test code-generator-error-message
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var a = 1
var a = 2")))))
               "Shovel error in file 'test.shr' at line 3, column 5: Variable 'a' is already defined in this frame in file '\"test.shr\"', at line 2, column 5.
file 'test.shr' line 3: var a = 2
file 'test.shr' line 3:     ^

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
b = 3
")))))
               "Shovel error in file 'test.shr' at line 2, column 1: Undefined variable 'b'.
file 'test.shr' line 2: b = 3
file 'test.shr' line 2: ^^^^^

"))
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
b + 1
")))))
               "Shovel error in file 'test.shr' at line 2, column 1: Undefined variable 'b'.
file 'test.shr' line 2: b + 1
file 'test.shr' line 2: ^

")))

(test vm-error-message
  (is (string= (with-output-to-string (str)
                 (let ((*standard-output* str))
                   (shovel:run-code
                    (list (shovel:stdlib)
                          (shovel-types:make-shript-file :name "test.shr"
                                                         :contents "
var g = fn x x + 2
var f = fn x g(x) + 2
f('1')")))))
               "Shovel error in file 'test.shr' at line 2, column 14: Arguments must have the same type (numbers or strings or arrays).

Current stack trace:
file 'test.shr' line 2: var g = fn x x + 2
file 'test.shr' line 2:              ^^^^^
file 'test.shr' line 3: var f = fn x g(x) + 2
file 'test.shr' line 3:              ^^^^
file 'test.shr' line 4: f('1')
file 'test.shr' line 4: ^^^^^^

Current environment:
x = \"1\"
stdlib = hash(\"filter\", [...callable...], \"forEach\", [...callable...], \"forEachWithIndex\", [...callable...], \"forIndex\", [...callable...], \"map\", [...callable...], \"mapWithIndex\", [...callable...], \"max\", [...callable...], \"min\", [...callable...], \"reduceFromLeft\", [...callable...], \"reverse\", [...callable...], \"sort\", [...callable...], \"while\", [...callable...])
g = [...callable...]
f = [...callable...]


")))

(defun run-tests ()
  (fiveam:run! :shovel-tests))
