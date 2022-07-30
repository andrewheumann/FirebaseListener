# FirebaseListener

A barebones grasshopper component for listening live to changes to a Firebase Realtime Database, utilizing https://github.com/step-up-labs/firebase-database-dotnet.

## Note: 
This tool makes a few assumptions about the way your database is structured. If your top level Key is named "Data", it expects "Data" to have children which are all objects, like so:
```
Data
  - id-1:
      some-property: 5
      some-other-property: 10
  - id-2:
      some-property: 19
      some-other-property: 12
  - id-3:
      some-property: 45
      some-other-property: 2
```

Rather than the keys immediately under `Data` being simple objects like strings or numbers. It might work with other structures, but it's untested! 
