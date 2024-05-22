# Stripe E-Commerce sample project 

To run this project you will need to install dotnet core SDK `8.0.105` you can install it with the following on ubuntu 24.04:

```bash
# other distros, instruction available here: 
# https://learn.microsoft.com/en-ca/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

Once you have it installed, you can run the website with:

```bash 
cd website
dotnet run
```

To export a CSV to stripe using the CLI tool, run the following:

```bash
cd cli
dotnet run ./Resources/sample-data.csv
```

> N.B. You may add more CSV files as long as they reside in the resource folder, or you can have them in the working directory if you're executing the compiled executable directly.

Enjoy!