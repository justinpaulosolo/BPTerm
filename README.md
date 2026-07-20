**TODO**
- [x] SASAL auth + hardcoded Hello()
- [x] Design decoded-value representaion
- [x] Generic writer (`WriteByte`,`WriteUInt16/32`,`WriteString`,`WriteObjectPath`,`WriteSignature`, `Align`)
- [x] Signature Parser (recursive descent, `"a{sv}"` -> array-of-(dic-entry-of-string-and-variant))
- [x] Generic Reader, driven by the parsed signature
- [ ] Call `GetManagedObjects()` decode `a{oa{sa{sv}}}`
- [ ] Async I/O: background read loop + pending call table, distinguish METHOD_RETURN/ERROR/SIGNAL
- [ ] `AddMatch` + signal dispatch
- [ ] Bluetooth adapter control (generic)
- [ ] Find the actual device
- [ ] GATT walk

## D-Bus message wire layout

Layout of a full message on the wire, and which read call pulls out which piece (used when
reading a reply — `Message`'s writer produces this same layout in reverse):

```
Offset:   0       4       8      12      16              16+H         align8         16+H+pad+bodyLen
          |-------|-------|-------|-------|----- ... -----|-----|------|----- ... -----|
          | endian| body  |serial | header|  header fields |pad |      |     body       |
          | type  | length|       | fields |     data       |    |      |                |
          | flags |  (u)  |  (u)  | length |  (H bytes)      |    |      |  (bodyLen bytes)|
          | ver   |       |       |  (u)   |                 |    |      |                |
          |<--- 4 bytes ->|<-4bytes->|<-4bytes->|<---- H bytes -->|<pad>|<---- bodyLen bytes ---->|

Call 1: ReceiveExactly(12)      → [ endian,type,flags,ver | body length | serial ]   offsets 0-11
Call 2: ReceiveExactly(4)       → [ header fields length (H) ]                       offsets 12-15
Call 3: ReceiveExactly(H)       → [ header fields data ]                             offsets 16-(16+H-1)
Call 4: ReceiveExactly(pad)     → [ padding to next multiple of 8 ]                  offsets (16+H)-(align8-1)
Call 5: ReceiveExactly(bodyLen) → [ body ]                                           offsets align8-end
```

`bodyLength` and `H` are only known after calls 1 and 2 respectively — everything past that is
arithmetic on numbers just read, which is why the message can't be read in one fixed-size call.