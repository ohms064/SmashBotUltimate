FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
WORKDIR /app

# Copy csproj and NuGet.config then run dotnet restore as distinct layers
COPY nuget.config *.csproj ./
RUN dotnet restore 

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app

# Build environment (Replace your token here if you need it hardcoded everytime!)
#ARG token=lol_no_token_here_skid_git_gud_lmao
#ENV DISCORD_TOKEN=$token

# Install packages
RUN apt-get update -y 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
EXPOSE 80

COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "SmashBotUltimate.dll"]

#RUN THE FOLLOWING COMMAND
#docker run -e smashbot_token=NzAxNTc1MTcyNTE1MTY4MzU4.XsN3cQ.J3F515iIhPSEslawCHdWyq4J5Oc -p 8080:8080 -d ohms064/smashbotultimate as smashbot
#docker run -e smashbot_token=NzE5MDkxMjIyMTI3NTc1MTMx.XtyX9A.TH658wGxs4Hs2v_rg2DzY7_Y78o -p 8080:8080 -d ohms064/smashbotultimate as smashbot_debug
#docker build -t ohms064/smashbotultimate .