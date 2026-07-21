## D-Bus message wire layout
<img width="1440" height="520" alt="image" src="https://github.com/user-attachments/assets/6aeabc72-f1d7-4007-b937-a03386cabcf8" />

**TODO**
- [x] SASAL auth + hardcoded Hello()
- [x] Design decoded-value representaion
- [x] Generic writer (`WriteByte`,`WriteUInt16/32`,`WriteString`,`WriteObjectPath`,`WriteSignature`, `Align`)
- [x] Signature Parser (recursive descent, `"a{sv}"` -> array-of-(dic-entry-of-string-and-variant))
- [x] Generic Reader, driven by the parsed signature
- [x] Call `GetManagedObjects()` decode `a{oa{sa{sv}}}`
- [ ] Async I/O: background read loop + pending call table, distinguish METHOD_RETURN/ERROR/SIGNAL
- [ ] `AddMatch` + signal dispatch
- [ ] Bluetooth adapter control (generic)
- [ ] Find the actual device
- [ ] GATT walk
