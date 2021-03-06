;;;; shovel.asd

(asdf:defsystem #:shovel
  :depends-on (#:alexandria
               #:split-sequence
               #:fiveam
               #:cl-messagepack
               #:ironclad
               #:babel)
  :components
  ((:module
    "common-lisp"
    :components
    ((:module
      "src"
      :components
      ((:file "package")
       (:file "shovel-types" :depends-on ("package"))
       (:file "shovel-public-types" :depends-on ("package"))
       (:file "utils" :depends-on ("package"))
       (:module "compiler"
                :depends-on ("package" "shovel-types" "utils")
                :components ((:file "types")
                             (:file "tokenizer" :depends-on ("types"))
                             (:file "parser" :depends-on ("types"))
                             (:file "code-generator" :depends-on ("types"))
                             (:file "compiler")))
       (:module "vm"
                :depends-on ("package" "shovel-types")
                :components ((:file "prim0")
                             (:file "vm" :depends-on ("prim0"))))
       (:file "shovel" :depends-on ("package"
                                    "shovel-types"
                                    "shovel-public-types"
                                    "compiler"
                                    "vm"))))
     #-shovel-production
     (:module
      "tests"
      :depends-on ("src")
      :components ((:file "package")
                   (:file "tests" :depends-on ("package"))))))))
