﻿using DuetAPI.Commands;
using DuetAPI.Machine;
using DuetControlServer.FileExecution;
using System.IO;
using System.Threading.Tasks;

namespace DuetControlServer.Codes
{
    /// <summary>
    /// Static class that processes M-codes in the control server
    /// </summary>
    public static class MCodes
    {
        /// <summary>
        /// Process an M-code that should be interpreted by the control server
        /// </summary>
        /// <param name="code">Code to process</param>
        /// <returns>Result of the code if the code completed, else null</returns>
        public static async Task<CodeResult> Process(Code code)
        {
            switch (code.MajorNumber)
            {
                // Start a file print
                case 32:
                    string file = await FilePath.ToPhysical(code.GetUnprecedentedString());
                    await Print.Start(file);
                    break;

                // Run Macro File
                case 98:
                    CodeParameter pParam = code.GetParameter('P');
                    if (pParam != null)
                    {
                        string path = await FilePath.ToPhysical(pParam.AsString);
                        if (File.Exists(path))
                        {
                            MacroFile macro = new MacroFile(path, code.Channel, code.SourceConnection);
                            return await macro.RunMacro();
                        }
                    }
                    return new CodeResult();

                // Return from macro
                case 99:
                    if (!MacroFile.AbortLastFile(code.Channel))
                    {
                        return new CodeResult(DuetAPI.MessageType.Error, "Not executing a macro file");
                    }
                    return new CodeResult();

                // Emergency Stop
                case 112:
                    SPI.Interface.RequestEmergencyStop();
                    using (await Model.Provider.AccessReadWrite())
                    {
                        Model.Provider.Get.State.Status = MachineStatus.Halted;
                    }
                    break;

                // Reset controller. Process this here only if the machine has performed an emergency stop
                case 999:
                    using (await Model.Provider.AccessReadOnly())
                    {
                        if (Model.Provider.Get.State.Status == MachineStatus.Halted)
                        {
                            SPI.Interface.RequestReset();
                            return new CodeResult();
                        }
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// React to an executed M-code before its result is returend
        /// </summary>
        /// <param name="code">Code processed by RepRapFirmware</param>
        /// <param name="result">Result that it generated</param>
        /// <returns>Asynchronous task</returns>
        public static async Task CodeExecuted(Code code, CodeResult result)
        {
            if (!result.IsSuccessful)
            {
                return;
            }

            switch (code.MajorNumber)
            {
                // Absolute extrusion
                case 82:
                    using (await Model.Provider.AccessReadWrite())
                    {
                        Model.Provider.Get.Channels[code.Channel].RelativeExtrusion = false;
                    }
                    break;

                // Relative extrusion
                case 83:
                    using (await Model.Provider.AccessReadWrite())
                    {
                        Model.Provider.Get.Channels[code.Channel].RelativeExtrusion = false;
                    }
                    break;
            }
        }
    }
}