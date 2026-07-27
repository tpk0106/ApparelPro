# OPTIONAL backend dev container image (advanced path only — see
# SETUP-GUIDE.md Step 5). Default workflow runs the backend directly in
# Visual Studio on your host instead.
FROM mcr.microsoft.com/dotnet/sdk:10.0

# EF Core CLI — lets you run migrations from inside the container.
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# vsdbg — Visual Studio's remote debugger, needed so VS's Container Tools /
# Docker Compose launch profile can attach a real breakpoint-capable
# debugger to the API process running inside this container.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
EXPOSE 5000 5001
CMD ["sleep", "infinity"]
