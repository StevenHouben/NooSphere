
Mono files required to run Zeroconf. These files need to be referenced in the ActivitySystem project and copied
to the general application output folder, as they are both needed fo deployment. Note that Visual studio does
not do this automatically as one dll is a wrapper over the other one. so manually copy both files into the
bin folder of the application.

These binaries were custom compiled from source at https://github.com/vdaron/Mono.Zeroconf